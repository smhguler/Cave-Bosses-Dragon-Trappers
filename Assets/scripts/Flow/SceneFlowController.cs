using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Prototip sahne geçişlerini yönetir. Yükleme başarısızsa aynı sahnede UI swap yapar.
/// </summary>
public class SceneFlowController : MonoBehaviour
{
    public static SceneFlowController Instance { get; private set; }

    static bool sceneLoadedHooked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSceneLoadedHook();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LoadMainHub() => Navigate(SceneNames.MainHub);
    public void LoadMarket() => Navigate(SceneNames.Market);
    public void LoadCave() => Navigate(SceneNames.Cave);

    public static void Navigate(string sceneName)
    {
        if (TryLoadScene(sceneName))
            return;

        if (SceneFlowWarningGate.ShouldLogFallback(sceneName))
        {
            Debug.LogWarning(
                $"[SceneFlow] '{sceneName}' Build Settings'te yok veya yüklenemedi. UI fallback kullanılıyor.");
        }

        if (SceneFlowWarningGate.ShouldLogBuildSettingsHintOnce())
        {
            Debug.LogWarning(
                "[SceneFlow] DragonTrappers > Add Prototype Scenes to Build Settings çalıştırın.");
        }

        PrototypeUiFactory.SwitchSceneUi(sceneName);
    }

    static bool TryLoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
            return false;

        EnsureSceneLoadedHook();

        try
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SceneFlow] LoadScene exception: {ex.Message}");
            return false;
        }
    }

    static void EnsureSceneLoadedHook()
    {
        if (sceneLoadedHooked)
            return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneLoadedHooked = true;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid())
            return;

        var sceneName = scene.name;
        if (sceneName != SceneNames.MainHub
            && sceneName != SceneNames.Market
            && sceneName != SceneNames.Cave)
        {
            return;
        }

        PrototypeUiFactory.EnsureCamera();
        PrototypeUiFactory.EnsureSceneUi(sceneName);
    }
}
