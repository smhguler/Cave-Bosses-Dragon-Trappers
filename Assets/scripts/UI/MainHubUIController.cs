using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana oyun ekranı butonlarını IGameActionsService'e bağlar.
/// Tuzak / güvenli oda aksiyonları yalnızca Cave ekranındadır.
/// </summary>
public class MainHubUIController : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] Button goMarketButton;
    [SerializeField] Button enterCaveButton;
    [SerializeField] Button endDayButton;

    [Header("Feedback")]
    [SerializeField] Text statusText;

    public void Configure(Button market, Button cave, Button endDay, Text status)
    {
        goMarketButton = market;
        enterCaveButton = cave;
        endDayButton = endDay;
        statusText = status;
        RebindButtons();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += HandleMessage;
        if (AreButtonsAssigned())
            BindButtons();
        HandleMessage("Ana ekran hazır.");
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= HandleMessage;
        UnbindButtons();
    }

    void RebindButtons()
    {
        UnbindButtons();
        BindButtons();
    }

    bool AreButtonsAssigned() =>
        goMarketButton != null
        && enterCaveButton != null
        && endDayButton != null;

    void BindButtons()
    {
        Bind(goMarketButton, OnGoMarket);
        Bind(enterCaveButton, OnEnterCave);
        Bind(endDayButton, OnEndDay);
    }

    void UnbindButtons()
    {
        Unbind(goMarketButton, OnGoMarket);
        Unbind(enterCaveButton, OnEnterCave);
        Unbind(endDayButton, OnEndDay);
    }

    static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    static void Unbind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
    }

    void OnGoMarket() => GetActions()?.GoToMarket();
    void OnEnterCave() => GetActions()?.EnterCave();
    void OnEndDay() => GetActions()?.EndDay();

    IGameActionsService GetActions() => GameSession.Instance?.Actions;

    void HandleMessage(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
