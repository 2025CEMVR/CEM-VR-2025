using UnityEngine;
using UnityEditor;

public class TriangleCounterWindow : EditorWindow
{
    [MenuItem("Window/Triangle Counter")]
    public static void ShowWindow()
    {
        GetWindow<TriangleCounterWindow>("Triangle Counter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Triangle Counter", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Triangle Count"))
        {
            Repaint();
        }

        int totalTriangles = CalculateTotalTriangles();
        EditorGUILayout.LabelField("Total Triangles in Scene (with LOD):", totalTriangles.ToString());
    }

    private int CalculateTotalTriangles()
    {
        int triangleCount = 0;
        Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
        if (sceneCamera == null)
        {
            Debug.LogWarning("No active scene view camera found.");
            return 0;
        }

        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null && IsVisible(meshFilter.gameObject, sceneCamera))
            {
                triangleCount += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMesh in skinnedMeshRenderers)
        {
            if (skinnedMesh.sharedMesh != null && IsVisible(skinnedMesh.gameObject, sceneCamera))
            {
                triangleCount += skinnedMesh.sharedMesh.triangles.Length / 3;
            }
        }

        LODGroup[] lodGroups = FindObjectsOfType<LODGroup>();
        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();
            if (lods.Length > 0)
            {
                float distance = Vector3.Distance(sceneCamera.transform.position, lodGroup.transform.position);
                float relativeHeight = Mathf.Clamp01(1.0f - (distance / lodGroup.size));
                foreach (var lod in lods)
                {
                    if (relativeHeight >= lod.screenRelativeTransitionHeight)
                    {
                        foreach (Renderer renderer in lod.renderers)
                        {
                            if (renderer is MeshRenderer meshRenderer)
                            {
                                MeshFilter filter = meshRenderer.GetComponent<MeshFilter>();
                                if (filter?.sharedMesh != null)
                                {
                                    triangleCount += filter.sharedMesh.triangles.Length / 3;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        return triangleCount;
    }

    private bool IsVisible(GameObject obj, Camera cam)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return false;
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), renderer.bounds);
    }
}
