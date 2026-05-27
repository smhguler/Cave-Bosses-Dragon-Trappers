#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;

/// <summary>
/// MainHub / Market / Cave sahnelerini Build Settings'e ekler (eksik olanları).
/// </summary>
public static class PrototypeBuildSettingsUtility
{
    const string ScenesFolder = "Assets/Scenes";

    public static readonly string[] PrototypeScenePaths =
    {
        $"{ScenesFolder}/{SceneNames.MainHub}.unity",
        $"{ScenesFolder}/{SceneNames.Market}.unity",
        $"{ScenesFolder}/{SceneNames.Cave}.unity",
    };

    [MenuItem("DragonTrappers/Add Prototype Scenes to Build Settings")]
    public static void AddPrototypeScenesToBuildSettings()
    {
        var added = EnsurePrototypeScenesInBuildSettings();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "DragonTrappers",
            added > 0
                ? $"{added} sahne Build Settings'e eklendi."
                : "Tüm prototip sahneleri zaten Build Settings'te.",
            "Tamam");
    }

    public static int EnsurePrototypeScenesInBuildSettings()
    {
        var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var knownPaths = new HashSet<string>();
        for (var i = 0; i < existing.Count; i++)
            knownPaths.Add(existing[i].path);

        var added = 0;
        for (var i = 0; i < PrototypeScenePaths.Length; i++)
        {
            var path = PrototypeScenePaths[i];
            if (!File.Exists(path))
                continue;

            if (knownPaths.Contains(path))
                continue;

            existing.Add(new EditorBuildSettingsScene(path, true));
            knownPaths.Add(path);
            added++;
        }

        if (added > 0)
            EditorBuildSettings.scenes = existing.ToArray();

        return added;
    }

    public static bool AreAllPrototypeScenesInBuildSettings()
    {
        var knownPaths = new HashSet<string>();
        var scenes = EditorBuildSettings.scenes;
        for (var i = 0; i < scenes.Length; i++)
            knownPaths.Add(scenes[i].path);

        for (var i = 0; i < PrototypeScenePaths.Length; i++)
        {
            if (!File.Exists(PrototypeScenePaths[i]))
                continue;

            if (!knownPaths.Contains(PrototypeScenePaths[i]))
                return false;
        }

        return true;
    }
}
#endif
