using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Profiling;

public class DrawCallPerformanceTest
{
    private const float TIMEOUT_DURATION = 60f; // 60 seconds max for scene load
    private const int MAX_ACCEPTABLE_DRAW_CALLS = 1000; // Set your performance target
    private const double MAX_STD_DEVIATION = 50.0; // Acceptable draw call stability

    private ProfilerRecorder drawCallsRecorder;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Start recording draw calls
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

        // Load scene and wait for it to finish
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

        // Let scene stabilize
        yield return new WaitForSeconds(2f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        drawCallsRecorder.Dispose();
        yield return null;
    }

    [UnityTest]
    public IEnumerator DrawCalls_Are_Within_Limit()
    {
        yield return new WaitForSeconds(1f); // Let any startup rendering finish

        long drawCalls = drawCallsRecorder.LastValue;
        Debug.Log($"[Draw Call Test] Draw calls: {drawCalls}");

        Assert.LessOrEqual(drawCalls, MAX_ACCEPTABLE_DRAW_CALLS,
            $"Draw calls ({drawCalls}) exceeded maximum allowed ({MAX_ACCEPTABLE_DRAW_CALLS})");
    }

    [UnityTest]
    public IEnumerator DrawCalls_Stable_Over_Time()
    {
        int sampleCount = 30;
        long[] samples = new long[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = drawCallsRecorder.LastValue;
            yield return new WaitForSeconds(0.1f); // Delay to get frame differences
        }

        // Calculate average and standard deviation
        double avg = 0;
        foreach (var value in samples) avg += value;
        avg /= sampleCount;

        double variance = 0;
        foreach (var value in samples) variance += Mathf.Pow(value - (float)avg, 2);
        variance /= sampleCount;

        double stdDev = Mathf.Sqrt((float)variance);
        Debug.Log($"[Draw Call Test] Avg: {avg}, Std Dev: {stdDev}");

        Assert.LessOrEqual(stdDev, MAX_STD_DEVIATION,
            $"Draw call variation too high: {stdDev} > {MAX_STD_DEVIATION}");
    }
}
