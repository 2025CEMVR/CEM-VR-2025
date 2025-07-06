using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTester
{
    [MenuItem("Tools/Test Multi-Scene Load")]
    public static void TestMultiSceneLoad()
    {
        Debug.Log("=== Starting Multi-Scene Load Test ===");

        // Paths relative to your Assets folder
        string loadScenePath = "Assets/Scenes/LoadScene.unity";
        string cemeteryScenePath = "Assets/Scenes/CemeteryAssetsFull.unity";

        // Open LoadScene.unity in Single mode
        var loadScene = EditorSceneManager.OpenScene(loadScenePath, OpenSceneMode.Single);
        Debug.Log($"Opened scene: {loadScene.path}");

        // Load CemeteryAssetsFull.unity additively
        var cemeteryScene = EditorSceneManager.OpenScene(cemeteryScenePath, OpenSceneMode.Additive);
        Debug.Log($"Additively loaded scene: {cemeteryScene.path}");

        // Set the active scene (optional but often helpful)
        EditorSceneManager.SetActiveScene(cemeteryScene);

        Debug.Log("Both scenes should now be loaded.");

        // Check for DatabaseControl in LoadScene
        var dbControlObj = GameObject.Find("DatabaseControl");
        if (dbControlObj != null)
        {
            Debug.Log("✅ Found DatabaseControl GameObject.");
        }
        else
        {
            Debug.LogWarning("❌ DatabaseControl GameObject NOT FOUND!");
        }

        // Check for HSdata in CemeteryAssetsFull
        var hsData = Object.FindObjectOfType<HSdata>();
        if (hsData != null)
        {
            Debug.Log("✅ Found HSdata component in scene.");
        }
        else
        {
            Debug.LogWarning("❌ HSdata component NOT FOUND!");
        }

        Debug.Log("=== Multi-Scene Load Test Complete ===");
    }
}
