using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Market alış butonları; ejderha satışı MarketDragonSalesPresenter üzerinden.
/// </summary>
public class MarketUIController : MonoBehaviour
{
    [SerializeField] Button buyTrapButton;
    [SerializeField] Button buyRabbitButton;
    [SerializeField] Button backButton;
    [SerializeField] Text statusText;

    public void Configure(Button buyTrap, Button buyRabbit, Button back, Text status)
    {
        buyTrapButton = buyTrap;
        buyRabbitButton = buyRabbit;
        backButton = back;
        statusText = status;
        RebindButtons();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += HandleMessage;
        if (AreButtonsAssigned())
            BindButtons();
        HandleMessage("Market — alış ve ejderha satışı.");
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= HandleMessage;
        UnbindButtons();
        HuntSelectionState.Clear();
    }

    void RebindButtons()
    {
        UnbindButtons();
        BindButtons();
    }

    bool AreButtonsAssigned() =>
        buyTrapButton != null && buyRabbitButton != null && backButton != null;

    void BindButtons()
    {
        Bind(buyTrapButton, OnBuyTrap);
        Bind(buyRabbitButton, OnBuyRabbit);
        Bind(backButton, OnBack);
    }

    void UnbindButtons()
    {
        Unbind(buyTrapButton, OnBuyTrap);
        Unbind(buyRabbitButton, OnBuyRabbit);
        Unbind(backButton, OnBack);
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

    void OnBuyTrap() => GameSession.Instance?.Actions.BuyTrap();
    void OnBuyRabbit() => GameSession.Instance?.Actions.BuyRabbit();
    void OnBack() => GameSession.Instance?.Actions.ReturnToMainHub();

    void HandleMessage(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
