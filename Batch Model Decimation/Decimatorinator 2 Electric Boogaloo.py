bl_info = {
    "name": "Batch Decimator with Poly Count Check",
    "author": "Your Name",
    "version": (1, 0),
    "blender": (2, 93, 0),
    "location": "View3D > Sidebar > Batch Decimator",
    "description": "Batch import models, check per-object poly count, decimate high-poly objects, and export.",
    "warning": "",
    "doc_url": "",
    "category": "Import-Export",
}

import bpy
import os
from bpy.props import StringProperty, FloatProperty, EnumProperty, IntProperty
from bpy.types import Operator, Panel, PropertyGroup


# Properties: input folder, output folder, decimation settings, and poly count threshold

class BATCHDECIMATOR_Properties(PropertyGroup):
    input_folder: StringProperty(
        name="Input Folder",
        description="Folder containing the models to process",
        default="",
        subtype='DIR_PATH'
    )
    output_folder: StringProperty(
        name="Output Folder",
        description="Folder to store the processed models",
        default="",
        subtype='DIR_PATH'
    )
    decimate_ratio: FloatProperty(
        name="Decimation Ratio",
        description="Fraction of faces to keep (0-1) if decimation is applied",
        default=0.5,
        min=0.0,
        max=1.0
    )
    decimate_method: EnumProperty(
        name="Decimation Method",
        description="Choose the decimation method",
        items=[
            ('COLLAPSE', "Collapse", "Use the 'Collapse' method"),
            ('UNSUBDIV', "Un-Subdivide", "Use the 'Un-Subdivide' method"),
            ('PLANAR', "Planar", "Use the 'Planar' method")
        ],
        default='COLLAPSE'
    )
    max_polygons: IntProperty(
        name="Max Polygons per Object",
        description="If a mesh object has more than this many polygons, it will be decimated. Otherwise, it will be exported unchanged.",
        default=10000,
        min=0
    )


# Operator: Batch import, check poly count, decimate if needed, and export

class BATCHDECIMATOR_OT_decimate(Operator):
    bl_idname = "batchdecimator.decimate"
    bl_label = "Batch Decimate with Poly Check"
    
    def execute(self, context):
        props = context.scene.batchdecimator_props
        
        input_folder = props.input_folder
        output_folder = props.output_folder
        ratio = props.decimate_ratio
        method = props.decimate_method
        max_polygons = props.max_polygons
        
        # validate folder paths
        if not os.path.isdir(input_folder):
            self.report({'ERROR'}, "Invalid input folder")
            return {'CANCELLED'}
        if not os.path.isdir(output_folder):
            self.report({'ERROR'}, "Invalid output folder")
            return {'CANCELLED'}
        
        # get all files in the input folder
        files = [f for f in os.listdir(input_folder) if os.path.isfile(os.path.join(input_folder, f))]
        
        for f in files:
            filepath = os.path.join(input_folder, f)
            # Only process OBJ or FBX files (adjust if needed)
            if not (f.lower().endswith(".obj") or f.lower().endswith(".fbx")):
                print(f"Skipping unsupported file format: {f}")
                continue
            
            # clear current scene objects
            bpy.ops.object.select_all(action='SELECT')
            bpy.ops.object.delete(use_global=False)
            
            print(f"\nProcessing file: {filepath}")
            try:
                if f.lower().endswith(".obj"):
                    bpy.ops.import_scene.obj(filepath=filepath)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.import_scene.fbx(filepath=filepath)
            except Exception as e:
                self.report({'WARNING'}, f"Failed to import {f}: {e}")
                continue
            
            # process each mesh object individually
            for obj in bpy.context.scene.objects:
                if obj.type == 'MESH':
                    polycount = len(obj.data.polygons)
                    print(f"Object '{obj.name}' poly count: {polycount}")
                    
                    if polycount > max_polygons:
                        print(f"Decimating '{obj.name}' (poly count {polycount} > {max_polygons})...")
                        bpy.ops.object.select_all(action='DESELECT')
                        obj.select_set(True)
                        bpy.context.view_layer.objects.active = obj
                        mod = obj.modifiers.new(name="DecimateMod", type='DECIMATE')
                        mod.decimate_type = method
                        if method == 'COLLAPSE':
                            mod.ratio = ratio
                        elif method == 'UNSUBDIV':
                            mod.iterations = 2  # adjust as needed
                        elif method == 'PLANAR':
                            mod.angle_limit = 0.0174533 * 5.0  # ~5 degrees in radians
                        try:
                            bpy.ops.object.modifier_apply(modifier=mod.name)
                        except Exception as e_mod:
                            self.report({'WARNING'}, f"Failed to apply decimation on {obj.name}: {e_mod}")
                    else:
                        print(f"Skipping decimation for '{obj.name}' (poly count {polycount} <= {max_polygons})")
            
            # export the scene to the output folder
            output_name = os.path.splitext(f)[0] + "_processed" + os.path.splitext(f)[1]
            output_path = os.path.join(output_folder, output_name)
            try:
                bpy.ops.object.select_all(action='DESELECT')
                for obj in bpy.context.scene.objects:
                    obj.select_set(True)
                if f.lower().endswith(".obj"):
                    bpy.ops.export_scene.obj(filepath=output_path, use_selection=True)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.export_scene.fbx(filepath=output_path, use_selection=True)
                print(f"Exported to: {output_path}")
            except Exception as e_exp:
                self.report({'WARNING'}, f"Failed to export {f}: {e_exp}")
                continue
        
        self.report({'INFO'}, "Batch decimation complete.")
        return {'FINISHED'}


# Panel: UI in the 3D View sidebar

class BATCHDECIMATOR_PT_panel(Panel):
    bl_label = "Batch Decimator"
    bl_idname = "BATCHDECIMATOR_PT_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Batch Decimator"
    
    def draw(self, context):
        layout = self.layout
        props = context.scene.batchdecimator_props
        
        layout.prop(props, "input_folder")
        layout.prop(props, "output_folder")
        layout.prop(props, "decimate_ratio")
        layout.prop(props, "decimate_method")
        layout.prop(props, "max_polygons")
        
        layout.operator("batchdecimator.decimate", text="Run Batch Decimate")


# Registration

classes = (
    BATCHDECIMATOR_Properties,
    BATCHDECIMATOR_OT_decimate,
    BATCHDECIMATOR_PT_panel,
)

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.batchdecimator_props = bpy.props.PointerProperty(type=BATCHDECIMATOR_Properties)

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    del bpy.types.Scene.batchdecimator_props

if __name__ == "__main__":
    register()
