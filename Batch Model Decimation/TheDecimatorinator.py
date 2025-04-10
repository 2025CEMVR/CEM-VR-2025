bl_info = {
    "name": "Decimatorinator",
    "author": "CIG",
    "version": (1, 0),
    "blender": (2, 93, 0),
    "location": "View3D > Tools",
    "description": "Import, DECIMATE, Export",
    "warning": "",
    "doc_url": "",
    "category": "Import-Export",
}

import bpy
import os
from bpy.props import StringProperty, FloatProperty, EnumProperty
from bpy.types import Operator, Panel, PropertyGroup
import addon_utils
addon_utils.enable("io_scene_obj")


class BATCHDECIMATOR_Properties(PropertyGroup):
    input_folder: StringProperty(
        name="Input Folder",
        description="Folder containing the models to decimate",
        default="",
        subtype='DIR_PATH'
    )
    output_folder: StringProperty(
        name="Output Folder",
        description="Folder to store the decimated models",
        default="",
        subtype='DIR_PATH'
    )
    decimate_ratio: FloatProperty(
        name="Decimation Ratio",
        description="Fraction of faces to keep (0-1) if using COLLAPSE",
        default=0.5,
        min=0.0,
        max=1.0
    )
    decimate_method: EnumProperty(
        name="Decimate Method",
        description="Choose the decimation method",
        items=[
            ('COLLAPSE', "Collapse", "Use the 'Collapse' method"),
            ('UNSUBDIV', "Un-Subdivide", "Use the 'Un-Subdivide' method"),
            ('PLANAR', "Planar", "Use the 'Planar' method")
        ],
        default='COLLAPSE'
    )


# operator for batch decimation

class BATCHDECIMATOR_OT_decimate(Operator):
    """Batch decimate models in a folder"""
    bl_idname = "batchdecimator.decimate"
    bl_label = "Batch Decimate"

    def execute(self, context):
        props = context.scene.batchdecimator_props
        
        input_folder = props.input_folder
        output_folder = props.output_folder
        ratio = props.decimate_ratio
        method = props.decimate_method

        # folder validation
        if not os.path.isdir(input_folder):
            self.report({'ERROR'}, "Invalid input folder")
            return {'CANCELLED'}
        if not os.path.isdir(output_folder):
            self.report({'ERROR'}, "Invalid output folder")
            return {'CANCELLED'}

        # get a list of files in the input folder
        files = [f for f in os.listdir(input_folder) if os.path.isfile(os.path.join(input_folder, f))]
        
        # process files
        for f in files:
            filepath = os.path.join(input_folder, f)

            # check extensions (.obj doesn't fucking work because Blender removed the import-export addon past version 3.5 so I wasted my goddamn time on implementing the .obj portion of this addon but it's cool, I'm not upset. I am still leaving all of the .obj portions of this in here.)
            if not (f.lower().endswith(".obj") or f.lower().endswith(".fbx")):
                print(f"Skipping unsupported file format: {f}")
                continue

            # clear existing objects in the current scene 
            bpy.ops.object.select_all(action='SELECT')
            bpy.ops.object.delete(use_global=False)

            #  import the model (one at a time)
            print(f"\nImporting: {filepath}")
            try:
                if f.lower().endswith(".obj"):
                    bpy.ops.import_scene.obj(filepath=filepath)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.import_scene.fbx(filepath=filepath)
            except Exception as e:
                self.report({'WARNING'}, f"Failed to import {f}: {e}")
                continue

            # --- DECIMATE DECIMATE DECIMATE ---
            all_objs = list(bpy.context.scene.objects)
            for obj in all_objs:
                if obj.type == 'MESH':
                    # make active
                    bpy.ops.object.select_all(action='DESELECT')
                    obj.select_set(True)
                    bpy.context.view_layer.objects.active = obj

                    mod = obj.modifiers.new("DecimateMod", 'DECIMATE')
                    mod.decimate_type = method
                    if method == 'COLLAPSE':
                        mod.ratio = ratio
                    elif method == 'UNSUBDIV':
                        # unsubdivide only
                        mod.iterations = 2
                    elif method == 'PLANAR':
                        # 'angle_limit' is in radians
                        mod.angle_limit = 0.0174533 * 5.0  # ~5 degrees

                    # apply the modifier
                    try:
                        bpy.ops.object.modifier_apply(modifier=mod.name)
                    except Exception as e_mod:
                        self.report({'WARNING'}, f"Failed to apply decimate on {obj.name}: {e_mod}")

            # export the decimated model 
            output_name = os.path.splitext(f)[0] + "_decimated" + os.path.splitext(f)[1]
            output_path = os.path.join(output_folder, output_name)

            print(f"Exporting to: {output_path}")
            try:
                # select all again so we export everything
                bpy.ops.object.select_all(action='DESELECT')
                for obj in bpy.context.scene.objects:
                    obj.select_set(True)

                if f.lower().endswith(".obj"):
                    bpy.ops.export_scene.obj(filepath=output_path, use_selection=True)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.export_scene.fbx(filepath=output_path, use_selection=True)
            except Exception as e_exp:
                self.report({'WARNING'}, f"Failed to export {f}: {e_exp}")
                continue

        self.report({'INFO'}, "Batch decimation complete.")
        return {'FINISHED'}


# UI

class BATCHDECIMATOR_PT_panel(Panel):
    """Creates a Panel in the scene context of the properties editor"""
    bl_label = "Decimatorinator"
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

        layout.operator("batchdecimator.decimate", text="Run Batch Decimate")


# registration

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
