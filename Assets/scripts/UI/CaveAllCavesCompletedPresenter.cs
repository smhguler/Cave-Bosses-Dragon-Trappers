using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tüm mağaralar tamamlandığında kazanma panelini gösterir.
/// UI-only: GameActionResult flags yerine status mesajı pattern'ini dinler.
/// </summary>
public class CaveAllCavesCompletedPresenter : MonoBehaviour
{
    [SerializeField] GameObject winPanel;

    public void Configure(GameObject panel)
    {
        winPanel = panel;
        SetVisible(false);
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += HandleMessage;

        // Scene açıldıktan hemen sonra status varsa yakalayabilmek için.
        HandleMessage(UiFeedbackLog.LastMessage);
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= HandleMessage;
    }

    void HandleMessage(string message)
    {
        if (winPanel == null)
            return;

        if (string.IsNullOrEmpty(message))
            return;

        // PrototypeGameLoop: "... Tum magara tamamlandi."
        var lower = message.ToLowerInvariant();
        var completed = lower.Contains("tum magara tamamlandi")
            || lower.Contains("magara tamamlandi");

        if (completed)
            SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        if (winPanel != null)
            winPanel.SetActive(visible);
    }
}

