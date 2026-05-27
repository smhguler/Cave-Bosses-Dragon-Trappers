using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Market alış butonlarında güncel tuzak / tavşan fiyatlarını gösterir.
/// </summary>
public class MarketPricePresenter : MonoBehaviour
{
    [SerializeField] Button buyTrapButton;
    [SerializeField] Button buyRabbitButton;

    public void Configure(Button buyTrap, Button buyRabbit)
    {
        buyTrapButton = buyTrap;
        buyRabbitButton = buyRabbit;
        Refresh();
    }

    void Update() => Refresh();

    void Refresh()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return;

        var economy = session.Game.economy;
        SetBuyLabel(buyTrapButton, "Tuzak Al", economy.trapBuyPrice);
        SetBuyLabel(buyRabbitButton, "Tavşan Al", economy.rabbitBuyPrice);
    }

    static void SetBuyLabel(Button button, string action, int price)
    {
        if (button == null)
            return;

        var text = button.GetComponentInChildren<Text>();
        if (text != null)
            text.text = $"{action} ({price} altın)";
    }
}
