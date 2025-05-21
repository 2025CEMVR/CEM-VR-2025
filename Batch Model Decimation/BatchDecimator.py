bl_info = {
    "name": "Batch Decimator with Poly Count Check",
    "author": "C. Greenspan",
    "version": (1, 0),
    "blender": (4, 1, 0),
    "location": "View3D > Sidebar > Batch Decimator",
    "description": "Batch import models, check per-object poly count, decimate high-poly objects, and export.",
    "category": "Import-Export",
}

import bpy
import os
from bpy.props import StringProperty, FloatProperty, EnumProperty, IntProperty
from bpy.types import Operator, Panel, PropertyGroup

class BATCHDECIMATOR_Properties(PropertyGroup):
    input_folder: StringProperty(name="Input Folder", subtype='DIR_PATH')
    output_folder: StringProperty(name="Output Folder", subtype='DIR_PATH')
    decimate_ratio: FloatProperty(name="Decimation Ratio", default=0.5, min=0.0, max=1.0)
    decimate_method: EnumProperty(
        name="Decimation Method",
        items=[
            ('COLLAPSE', "Collapse", ""),
            ('UNSUBDIV', "Un-Subdivide", ""),
            ('PLANAR', "Planar", "")
        ],
        default='COLLAPSE'
    )
    max_polygons: IntProperty(name="Max Polygons per Object", default=10000, min=0)

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

        if not os.path.isdir(input_folder):
            self.report({'ERROR'}, "Invalid input folder")
            return {'CANCELLED'}
        if not os.path.isdir(output_folder):
            self.report({'ERROR'}, "Invalid output folder")
            return {'CANCELLED'}

        files = [f for f in os.listdir(input_folder) if f.lower().endswith(('.obj', '.fbx'))]

        for f in files:
            filepath = os.path.join(input_folder, f)

            # Clear the scene
            bpy.ops.object.select_all(action='SELECT')
            bpy.ops.object.delete(use_global=False)

            try:
                if f.lower().endswith(".obj"):
                    bpy.ops.import_scene.obj(filepath=filepath)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.import_scene.fbx(filepath=filepath)
            except Exception as e:
                self.report({'WARNING'}, f"Import failed for {f}: {e}")
                continue

            mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']

            # 💥 Abort if nothing valid was imported
            if not mesh_objects:
                print(f"⚠️ No mesh objects imported from {f}. Skipping export.")
                self.report({'WARNING'}, f"No valid geometry in {f}")
                continue

            print(f"\n📦 Imported {f} — Found mesh objects:")
            for obj in mesh_objects:
                print(f"  • {obj.name} ({len(obj.data.polygons)} polys)")

            for obj in mesh_objects:
                polycount = len(obj.data.polygons)
                if polycount > max_polygons:
                    bpy.ops.object.select_all(action='DESELECT')
                    obj.select_set(True)
                    bpy.context.view_layer.objects.active = obj
                    bpy.ops.object.mode_set(mode='OBJECT')
                    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

                    mod = obj.modifiers.new(name="DecimateMod", type='DECIMATE')
                    mod.decimate_type = method

                    if method == 'COLLAPSE':
                        mod.ratio = ratio
                    elif method == 'UNSUBDIV':
                        mod.iterations = 2
                    elif method == 'PLANAR':
                        mod.angle_limit = 0.0174533 * 5.0

                    try:
                        bpy.ops.object.modifier_apply(modifier=mod.name)
                        print(f"✔️ Decimated '{obj.name}' to {len(obj.data.polygons)} polys")
                    except Exception as e_mod:
                        self.report({'WARNING'}, f"Modifier apply failed: {e_mod}")
                else:
                    print(f"⏩ Skipped decimation for '{obj.name}' — poly count under threshold")

            # Export
            output_name = os.path.splitext(f)[0] + "_processed" + os.path.splitext(f)[1]
            output_path = os.path.join(output_folder, output_name)

            try:
                bpy.ops.object.select_all(action='DESELECT')
                for obj in bpy.context.scene.objects:
                    if obj.type == 'MESH':
                        obj.select_set(True)

                if f.lower().endswith(".obj"):
                    bpy.ops.export_scene.obj(filepath=output_path, use_selection=True)
                elif f.lower().endswith(".fbx"):
                    bpy.ops.export_scene.fbx(filepath=output_path, use_selection=True)

                print(f"📤 Exported to: {output_path}")
            except Exception as e_exp:
                self.report({'WARNING'}, f"Export failed for {f}: {e_exp}")
                continue

        self.report({'INFO'}, "✅ Batch decimation complete.")
        return {'FINISHED'}

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
        layout.operator("batchdecimator.decimate")

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
