using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kalıcı oturum, sahne akışı ve prototip UI'nin sahne yüklendiğinde hazır olmasını sağlar.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    static bool sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        EnsureBootstrapObject();
        RegisterSceneHook();
        PrototypeUiFactory.EnsureCamera();
        PrototypeUiFactory.EnsureSceneUi(SceneManager.GetActiveScene().name);
    }

    static void EnsureBootstrapObject()
    {
        if (GameSession.Instance != null && SceneFlowController.Instance != null)
            return;

        var existing = Object.FindObjectOfType<GameBootstrap>();
        if (existing != null)
            return;

        var go = new GameObject("_GameBootstrap");
        go.AddComponent<GameBootstrap>();
    }

    static void RegisterSceneHook()
    {
        if (sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHookRegistered = true;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PrototypeUiFactory.EnsureCamera();
        PrototypeUiFactory.EnsureSceneUi(scene.name);
    }

    void Awake()
    {
        if (GameSession.Instance == null)
        {
            var sessionGo = new GameObject("GameSession");
            sessionGo.AddComponent<GameSession>();
        }

        if (SceneFlowController.Instance == null)
            gameObject.AddComponent<SceneFlowController>();

        Destroy(this);
    }
}
