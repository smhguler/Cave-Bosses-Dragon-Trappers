#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainHub, Market ve Cave sahnelerini kaydeder ve Build Settings'i günceller.
/// Menü: DragonTrappers > Setup Prototype UI Scenes
/// </summary>
public static class PrototypeUISetup
{
    const string ScenesFolder = "Assets/Scenes";

    [MenuItem("DragonTrappers/Setup Prototype UI Scenes")]
    public static void SetupAllScenes()
    {
        Directory.CreateDirectory(ScenesFolder);

        SaveScene(SceneNames.MainHub, () => PrototypeUiFactory.BuildMainHub());

        SaveScene(SceneNames.Market, () => PrototypeUiFactory.BuildMarket());
        SaveScene(SceneNames.Cave, () => PrototypeUiFactory.BuildCave());

        PrototypeBuildSettingsUtility.EnsurePrototypeScenesInBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "DragonTrappers",
            "MainHub, Market ve Cave sahneleri oluşturuldu.\nPlay için MainHub sahnesini açın.",
            "Tamam");

        EditorSceneManager.OpenScene($"{ScenesFolder}/{SceneNames.MainHub}.unity");
    }

    static void SaveScene(string sceneName, System.Action build)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        build();
        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{sceneName}.unity");
    }

}
#endif
