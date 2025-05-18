using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Unity.Profiling;

public class PerformanceTests
{
    private float elapsedTime = 0f;
    private const float TIMEOUT_DURATION = 60f; // 1 minute timeout
    private const int MAX_ACCEPTABLE_DRAW_CALLS = 1000; // Adjust this threshold based on your target performance
    private ProfilerRecorder drawCallsRecorder;

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
        try
        {
            // Make sure any previous recorder is disposed
            if (drawCallsRecorder.Valid)
            {
                drawCallsRecorder.Dispose();
            }

            // Initialize with a more specific profiler marker
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Renderer.DrawCall");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize ProfilerRecorder: {e.Message}");
            Assert.Fail($"ProfilerRecorder initialization failed: {e.Message}");
        }

        elapsedTime = 0f;
        var asyncOperation = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Single);
        float timer = 0f;
        while (!asyncOperation.isDone)
        {
            if (timer >= TIMEOUT_DURATION)
            {
                Assert.Fail($"Scene loading timed out after {TIMEOUT_DURATION} seconds");
            }
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(2f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        try
        {
            if (drawCallsRecorder.Valid)
            {
                drawCallsRecorder.Dispose();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during recorder cleanup: {e.Message}");
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator DrawCall_Count_Within_Acceptable_Range()
    {
        Assert.IsTrue(drawCallsRecorder.Valid, "ProfilerRecorder is not valid");

        // Wait a few frames to let everything settle
        yield return new WaitForSeconds(0.5f);

        try
        {
            long drawCalls = drawCallsRecorder.LastValue;
            Debug.Log($"Current draw calls: {drawCalls}");
            
            Assert.LessOrEqual(drawCalls, MAX_ACCEPTABLE_DRAW_CALLS, 
                $"Draw calls ({drawCalls}) exceeded maximum acceptable limit ({MAX_ACCEPTABLE_DRAW_CALLS})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading draw calls: {e.Message}");
            Assert.Fail($"Failed to read draw calls: {e.Message}");
        }
    }

    [UnityTest]
    public IEnumerator DrawCall_Count_Stable_Over_Time()
    {
        Assert.IsTrue(drawCallsRecorder.Valid, "ProfilerRecorder is not valid");

        int sampleCount = 10; // Reduced sample count
        long[] drawCallSamples = new long[sampleCount];
        
        try
        {
            // Collect samples
            for (int i = 0; i < sampleCount; i++)
            {
                drawCallSamples[i] = drawCallsRecorder.LastValue;
                yield return new WaitForSeconds(0.2f);
            }

            // Calculate average
            double average = 0;
            foreach (long sample in drawCallSamples)
            {
                average += sample;
            }
            average /= sampleCount;

            // Calculate standard deviation
            double variance = 0;
            foreach (long sample in drawCallSamples)
            {
                variance += Mathf.Pow(sample - (float)average, 2);
            }
            variance /= sampleCount;
            double standardDeviation = Mathf.Sqrt((float)variance);

            Debug.Log($"Draw calls - Average: {average}, Standard Deviation: {standardDeviation}");

            double maxAcceptableStdDev = 50.0;
            Assert.LessOrEqual(standardDeviation, maxAcceptableStdDev, 
                $"Draw call count variation ({standardDeviation}) exceeded acceptable standard deviation ({maxAcceptableStdDev})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during draw call sampling: {e.Message}");
            Assert.Fail($"Failed during draw call sampling: {e.Message}");
        }
    }

    [UnityTest]
    public IEnumerator Basic_Test_No_Scene_Load()
    {
        // Just wait a few frames
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(true, "Basic test completed");
    }

    [UnityTest]
    public IEnumerator Basic_Scene_Load_Test()
    {
        // Log current scene
        Debug.Log($"Current scene: {SceneManager.GetActiveScene().name}");
        
        // Start scene load
        var asyncOperation = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Single);
        
        // Wait for scene load
        float timer = 0f;
        float timeout = 10f;
        
        while (!asyncOperation.isDone)
        {
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Assert.Fail("Scene loading timed out");
                yield break;
            }
            yield return null;
        }

        // Verify scene loaded
        Assert.AreEqual("LoadScene", SceneManager.GetActiveScene().name);
        
        // Wait a bit to let scene stabilize
        yield return new WaitForSeconds(0.5f);
        
        // Get draw count
        int drawCount = UnityEngine.Graphics.drawCount;
        Debug.Log($"Draw count: {drawCount}");
        
        // Basic draw count check
        Assert.LessOrEqual(drawCount, 1000, "Draw count is too high");
    }
} 