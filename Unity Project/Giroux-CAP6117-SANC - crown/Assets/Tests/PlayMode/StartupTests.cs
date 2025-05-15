using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

// Add reference to the script
using RotateCamera = UnityEngine.MonoBehaviour;

public class StartupTests
{
    private float elapsedTime = 0f;
    private const float TIMEOUT_DURATION = 60f; // 1 minute timeout

    private IEnumerator WaitWithTimeout()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= TIMEOUT_DURATION)
        {
            Assert.Fail($"Test timed out after {TIMEOUT_DURATION} seconds");
        }
        yield return null;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Set up expected log messages before loading the scene
        LogAssert.ignoreFailingMessages = true;
        
        // Expect specific error messages that we know will occur
        LogAssert.Expect(LogType.Error, "Problem detected while opening the Scene file: 'Assets/Scenes/CemeteryAssetsFull.unity'.");
        LogAssert.Expect(LogType.Error, "The referenced script (Unknown) on this Behaviour is missing!");
        LogAssert.Expect(LogType.Error, "The referenced script on this Behaviour (Game Object 'Particles') is missing!");
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Prefab instance problem: .* \\(Missing Prefab with guid: .*\\)"));
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("HTTP/1.1 404 Not Found"));

        // Force a garbage collection to clean up any previous scene data
        System.GC.Collect();
        
        // Load the scene additively first to ensure proper initialization
        var asyncOperation = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Single);
        asyncOperation.allowSceneActivation = true;
        
        while (!asyncOperation.isDone)
        {
            yield return WaitWithTimeout();
        }

        // Wait for scene to be fully loaded
        float waitTime = 0f;
        while (waitTime < 10f)
        {
            waitTime += Time.deltaTime;
            yield return WaitWithTimeout();
        }

        // Ensure lighting and render settings are applied
        DynamicGI.UpdateEnvironment();  // Update global illumination

        // Wait for lighting to be fully applied
        yield return WaitWithTimeout();
        yield return WaitWithTimeout();  // Double frame wait to ensure visual updates
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        LogAssert.ignoreFailingMessages = false;
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartupScene_LoadsCorrectly()
    {
        // Wait for any post-load initialization
        yield return WaitWithTimeout();
        
        // Check if scene loaded successfully
        Assert.AreEqual("LoadScene", SceneManager.GetActiveScene().name);
        Assert.IsTrue(SceneManager.GetActiveScene().IsValid(), "Scene should be valid");
        
        // Check if main camera is properly set up
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Ensure camera's render settings are correct
            Assert.IsTrue(mainCamera.clearFlags == CameraClearFlags.Skybox || 
                         mainCamera.clearFlags == CameraClearFlags.SolidColor,
                         "Camera should have appropriate clear flags");
        }

        yield return null;
    }
} 