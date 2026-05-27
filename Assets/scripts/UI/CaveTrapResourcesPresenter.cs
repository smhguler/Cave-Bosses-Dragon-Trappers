using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mağara tuzak butonları yanında tuzak / tavşan stoğu ve tavşanlı tuzak bonus etiketi.
/// </summary>
public class CaveTrapResourcesPresenter : MonoBehaviour
{
    public static readonly Color DisabledTrapVisualColor = new Color(0.22f, 0.22f, 0.24f, 0.65f);

    static readonly Color NormalTrapColor = new Color(0.20f, 0.33f, 0.34f, 0.96f);
    static readonly Color BaitTrapColor = new Color(0.28f, 0.22f, 0.38f, 0.98f);
    static readonly Color BaitTrapHighlight = new Color(0.38f, 0.30f, 0.50f, 1f);

    [SerializeField] Button placeTrapButton;
    [SerializeField] Button placeTrapWithRabbitButton;
    [SerializeField] Text trapStockLabel;
    [SerializeField] Text rabbitStockLabel;
    [SerializeField] Text rabbitBonusLabel;

    string lastSignature = "";

    public void Configure(
        Button trap,
        Button baitTrap,
        Text trapStock,
        Text rabbitStock,
        Text rabbitBonus)
    {
        placeTrapButton = trap;
        placeTrapWithRabbitButton = baitTrap;
        trapStockLabel = trapStock;
        rabbitStockLabel = rabbitStock;
        rabbitBonusLabel = rabbitBonus;
        Refresh();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += OnMaybeChanged;
        Refresh();
    }

    void OnDisable() => UiFeedbackLog.OnMessage -= OnMaybeChanged;

    void Update()
    {
        var signature = BuildSignature();
        if (signature == lastSignature)
            return;

        lastSignature = signature;
        Refresh();
    }

    void OnMaybeChanged(string message)
    {
        lastSignature = "";
        Refresh();
    }

    void Refresh()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return;

        var inv = session.Game.inventory;

        if (trapStockLabel != null)
            trapStockLabel.text = $"Tuzak: {inv.traps}";

        if (rabbitStockLabel != null)
            rabbitStockLabel.text = $"Tavşan: {inv.rabbits}";

        if (rabbitBonusLabel != null)
            rabbitBonusLabel.text = "+ ejder şansı";

        var trapEnabled = inv.traps > 0;
        var baitEnabled = inv.traps > 0 && inv.rabbits > 0;

        ApplyTrapButtonVisual(placeTrapButton, NormalTrapColor, NormalTrapColor, trapEnabled);
        ApplyTrapButtonVisual(placeTrapWithRabbitButton, BaitTrapColor, BaitTrapHighlight, baitEnabled);
    }

    static void ApplyTrapButtonVisual(Button button, Color enabledNormal, Color enabledHighlight, bool enabled)
    {
        if (button == null)
            return;

        button.interactable = enabled;

        var image = button.GetComponent<Image>();
        var normal = enabled ? enabledNormal : DisabledTrapVisualColor;
        var highlight = enabled ? enabledHighlight : DisabledTrapVisualColor;
        var pressed = enabled
            ? new Color(enabledNormal.r * 0.7f, enabledNormal.g * 0.7f, enabledNormal.b * 0.7f, 1f)
            : DisabledTrapVisualColor;

        if (image != null)
            image.color = normal;

        var colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = highlight;
        colors.pressedColor = pressed;
        colors.selectedColor = highlight;
        colors.disabledColor = DisabledTrapVisualColor;
        button.colors = colors;
    }

    static string BuildSignature()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return "";

        var inv = session.Game.inventory;
        return inv.traps + "|" + inv.rabbits;
    }
}
