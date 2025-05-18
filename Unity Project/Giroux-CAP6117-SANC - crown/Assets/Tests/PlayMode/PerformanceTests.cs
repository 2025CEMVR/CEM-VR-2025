using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Profiling;

public class DrawCallPerformanceTests
{
    private const string SceneName = "CemeteryAssetsFull";
    private const float TIMEOUT_SECONDS = 30f;
    private const int MAX_ACCEPTABLE_DRAW_CALLS = 1000;
    private const double MAX_STDDEV = 50.0;

    private ProfilerRecorder drawCallRecorder;

    [OneTimeSetUp]
public void GlobalLogFilter()
{
    Application.logMessageReceived += (condition, stackTrace, type) =>
    {
        if (type == LogType.Error && condition.Contains("Missing Prefab") || condition.Contains("referenced script"))
        {
            // Ignore missing prefab/script errors from scene loading
            return;
        }

        if (type == LogType.Exception)
        {
            Debug.LogWarning($"[Test Exception] {condition}");
        }
    };
}

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        drawCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

        // Load scene
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;
            if (timer > TIMEOUT_SECONDS)
            {
                Assert.Fail($"Scene loading timed out after {TIMEOUT_SECONDS} seconds.");
                yield break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(2f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        drawCallRecorder.Dispose();
        yield return null;
    }

    [UnityTest]
    public IEnumerator DrawCalls_Within_Limit()
    {
        yield return WaitFrames(5);

        long drawCalls = drawCallRecorder.LastValue;
        Debug.Log($"[DrawCallTest] Draw Calls: {drawCalls}");

        Assert.LessOrEqual(drawCalls, MAX_ACCEPTABLE_DRAW_CALLS,
            $"Draw calls ({drawCalls}) exceeded max acceptable limit ({MAX_ACCEPTABLE_DRAW_CALLS}).");
    }

    [UnityTest]
    public IEnumerator DrawCalls_Stable_Over_Time()
    {
        const int sampleCount = 30;
        long[] samples = new long[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = drawCallRecorder.LastValue;
            yield return null;
        }

        double avg = 0;
        foreach (var val in samples) avg += val;
        avg /= sampleCount;

        double variance = 0;
        foreach (var val in samples)
            variance += Mathf.Pow((float)(val - avg), 2);
        variance /= sampleCount;

        double stdDev = Mathf.Sqrt((float)variance);

        Debug.Log($"[DrawCallTest] Avg: {avg:F2}, StdDev: {stdDev:F2}");

        Assert.LessOrEqual(stdDev, MAX_STDDEV,
            $"Draw call standard deviation too high: {stdDev:F2} (limit: {MAX_STDDEV})");
    }

    // Utility: wait N frames
    private IEnumerator WaitFrames(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
            yield return null;
    }
}
