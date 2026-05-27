using System.Collections.Generic;

/// <summary>
/// SceneFlow fallback uyarılarını oturum başına bir kez sınırlar.
/// </summary>
public static class SceneFlowWarningGate
{
    static readonly HashSet<string> WarnedScenes = new HashSet<string>();
    static bool missingBuildSettingsHintShown;

    public static bool ShouldLogFallback(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        return WarnedScenes.Add(sceneName);
    }

    public static bool ShouldLogActiveMismatch(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        return WarnedScenes.Add("mismatch:" + sceneName);
    }

    public static bool ShouldLogBuildSettingsHintOnce()
    {
        if (missingBuildSettingsHintShown)
            return false;

        missingBuildSettingsHintShown = true;
        return true;
    }

    public static void ResetForTests()
    {
        WarnedScenes.Clear();
        missingBuildSettingsHintShown = false;
    }
}
