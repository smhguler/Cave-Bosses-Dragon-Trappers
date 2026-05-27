#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Play Mode'da UI buton akışını GameActionsService üzerinden doğrular.
/// Menü: DragonTrappers / Verify UI Buttons (Play Mode)
/// </summary>
public static class PrototypeUiButtonPlayModeTest
{
    [MenuItem("DragonTrappers/Verify UI Buttons (Play Mode)")]
    public static void RunInPlayMode()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "DragonTrappers",
                "Önce Play Mode'a girin (MainHub veya boş sahne).",
                "Tamam");
            return;
        }

        RunAndLog();
    }

    [MenuItem("DragonTrappers/Verify UI Buttons (Play Mode)", true)]
    public static bool RunInPlayModeValidate() => EditorApplication.isPlaying;

    public static void RunAndLog()
    {
        var results = PrototypeUiFlowVerifier.RunAll();
        var report = PrototypeUiFlowVerifier.FormatReport(results);
        Debug.Log("[PrototypeUiFlowVerifier]\n" + report);

        var allPassed = true;
        foreach (var r in results)
        {
            if (!r.passed)
                allPassed = false;
        }

        if (!allPassed)
            Debug.LogError("[PrototypeUiFlowVerifier] Bazı adımlar başarısız.");
    }
}
#endif
