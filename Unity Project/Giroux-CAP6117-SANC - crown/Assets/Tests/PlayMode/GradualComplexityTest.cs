using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

/// <summary>
/// Gradual complexity test to isolate what causes the freeze
/// </summary>
public class GradualComplexityTest
{
    [UnityTest]
    public IEnumerator Step1_MinimalTest_ShouldWork()
    {
        Debug.Log("=== STEP 1: Minimal Test ===");
        yield return new WaitForSeconds(0.1f);
        Assert.Pass("Step 1 passed");
    }

    [UnityTest]
    public IEnumerator Step2_SceneInfo_ShouldWork()
    {
        Debug.Log("=== STEP 2: Scene Info ===");
        var currentScene = SceneManager.GetActiveScene();
        Debug.Log($"Current scene: {currentScene.name}");
        Debug.Log($"Scene count: {SceneManager.sceneCount}");
        yield return new WaitForSeconds(0.1f);
        Assert.Pass("Step 2 passed");
    }

    [UnityTest]
    public IEnumerator Step3_FindLoadMainScene_ShouldWork()
    {
        Debug.Log("=== STEP 3: Find LoadMainScene Component ===");
        
        // Try to find LoadMainScene component without loading anything
        var loadMainScene = Object.FindObjectOfType<LoadMainScene>();
        if (loadMainScene != null)
        {
            Debug.Log("Found LoadMainScene component in current scene");
        }
        else
        {
            Debug.Log("No LoadMainScene component found in current scene");
        }
        
        yield return new WaitForSeconds(0.1f);
        Assert.Pass("Step 3 passed");
    }

    [UnityTest]
    public IEnumerator Step4_FindDatabaseControl_ShouldWork()
    {
        Debug.Log("=== STEP 4: Find DatabaseControl Component ===");
        
        // Try to find DatabaseControl component
        var databaseControl = Object.FindObjectOfType<DatabaseControl>();
        if (databaseControl != null)
        {
            Debug.Log("Found DatabaseControl component in current scene");
        }
        else
        {
            Debug.Log("No DatabaseControl component found in current scene");
        }
        
        yield return new WaitForSeconds(0.1f);
        Assert.Pass("Step 4 passed");
    }

    [UnityTest]
    public IEnumerator Step5_LoadSceneAsync_ShouldWork()
    {
        Debug.Log("=== STEP 5: Load Scene Async (without activation) ===");
        
        // Try loading scene 1 (CemeteryAssetsFull) but don't activate it
        var asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false; // Don't activate yet
        
        Debug.Log("Scene loading started...");
        
        // Wait for scene to load but don't activate
        // When allowSceneActivation is false, isDone will never be true
        // Instead, we wait for progress to reach 0.9 (90%)
        float timeout = 30f; // 30 second timeout
        float startTime = Time.time;
        
        while (asyncOperation.progress < 0.9f)
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogError("Timeout waiting for scene to load!");
                Assert.Fail("Scene loading timed out");
                yield break;
            }
            
            Debug.Log($"Loading progress: {asyncOperation.progress * 100:F1}%");
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Scene loaded (but not activated)");
        Assert.Pass("Step 5 passed");
    }

    [UnityTest]
    public IEnumerator Step6_ActivateScene_ShouldWork()
    {
        Debug.Log("=== STEP 6: Activate Loaded Scene ===");
        
        // First load the scene without activation
        var asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;
        
        float timeout = 30f;
        float startTime = Time.time;
        
        while (!asyncOperation.isDone)
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogError("Timeout waiting for scene to load!");
                Assert.Fail("Scene loading timed out");
                yield break;
            }
            yield return null;
        }
        
        Debug.Log("Scene loaded, now activating...");
        
        // Now activate it
        asyncOperation.allowSceneActivation = true;
        
        // Wait a bit for activation
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"Scene activated. Total scenes: {SceneManager.sceneCount}");
        Assert.Pass("Step 6 passed");
    }

    [UnityTest]
    public IEnumerator Step7_FindComponentsInLoadedScene_ShouldWork()
    {
        Debug.Log("=== STEP 7: Find Components in Loaded Scene ===");
        
        // Load and activate scene
        var asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = true;
        
        float timeout = 30f;
        float startTime = Time.time;
        
        while (!asyncOperation.isDone)
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogError("Timeout waiting for scene to load!");
                Assert.Fail("Scene loading timed out");
                yield break;
            }
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        // Now try to find components
        var loadMainScene = Object.FindObjectOfType<LoadMainScene>();
        var databaseControl = Object.FindObjectOfType<DatabaseControl>();
        
        Debug.Log($"LoadMainScene found: {loadMainScene != null}");
        Debug.Log($"DatabaseControl found: {databaseControl != null}");
        
        Assert.Pass("Step 7 passed");
    }
} 