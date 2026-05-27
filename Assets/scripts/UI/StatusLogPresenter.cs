using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Son aksiyon mesajını status alanında okunur biçimde gösterir.
/// </summary>
public class StatusLogPresenter : MonoBehaviour
{
    const int MaxChars = 160;

    [SerializeField] Text statusText;

    public void Configure(Text status)
    {
        statusText = status;
        if (statusText != null)
        {
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Truncate;
            statusText.alignByGeometry = false;
            statusText.supportRichText = true;
        }
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += HandleMessage;
        RefreshDisplay();
    }

    void OnDisable() => UiFeedbackLog.OnMessage -= HandleMessage;

    void HandleMessage(string message) => RefreshDisplay();

    void RefreshDisplay()
    {
        if (statusText == null)
            return;

        var display = UiFeedbackLog.GetDisplayText();
        if (string.IsNullOrEmpty(display))
        {
            var session = GameSession.Instance;
            display = session != null ? "Hazır." : "";
        }

        var raw = UiFeedbackLog.LastMessage ?? "";
        var truncated = TruncateForPanel(display);

        var lower = raw.ToLowerInvariant();
        var allCaves = lower.Contains("tum magara tamamlandi") || lower.Contains("tamamlandi");
        var habitatCleared = lower.Contains("temizlendi");

        if (allCaves)
            statusText.text = $"<color=#4CFF6A><b>{truncated}</b></color>";
        else if (habitatCleared)
            statusText.text = $"<color=#FFD24A><b>{truncated}</b></color>";
        else
            statusText.text = truncated;
    }

    static string TruncateForPanel(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= MaxChars)
            return text;

        return text.Substring(0, MaxChars - 1) + "…";
    }
}
