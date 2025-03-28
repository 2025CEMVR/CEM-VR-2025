using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HeadstoneRotationHighlight
{
    private static readonly string prefabPath = "Assets/Prefabs/HSbundle.prefab";
    private static GameObject targetPrefab;
    private static bool isHighlightingEnabled = true;

    //Gets the prefab and ensures it exists!
    static HeadstoneRotationHighlight()
    {
        targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (targetPrefab == null)
        {
            Debug.LogError($"Prefab not found at path: {prefabPath}");
            return;
        }
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Toggle Headstone Highlight")]
    private static void ToggleHighlighting()
    {
        isHighlightingEnabled = !isHighlightingEnabled;
        Debug.Log($"Headstone Highlighting {(isHighlightingEnabled ? "Enabled" : "Disabled")}");
        SceneView.RepaintAll();
    }

    //words and such
    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isHighlightingEnabled || targetPrefab == null) return;

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                if (prefabRoot == targetPrefab && Mathf.Abs(obj.transform.eulerAngles.y) != 0)
                {
                    Handles.color = Color.red;
                    Handles.DrawWireCube(obj.transform.position, Vector3.one);
                    
                }
            }
        }

        // Force the SceneView to repaint and update
        SceneView.RepaintAll();
    }
}
