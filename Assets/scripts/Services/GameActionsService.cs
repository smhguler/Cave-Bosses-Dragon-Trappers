using UnityEngine;

/// <summary>
/// UI action adapter. Scene buttons call this layer; gameplay is delegated to PrototypeGameLoop.
/// </summary>
public class GameActionsService : IGameActionsService
{
    readonly GameSession session;

    public GameActionsService(GameSession session)
    {
        this.session = session;
    }

    public void GoToMarket()
    {
        if (session.Game.dayCycle.IsInMarket)
        {
            session.Publish("Market açılıyor.");
            SceneFlowController.Navigate(SceneNames.Market);
            return;
        }

        var result = session.RunAction(game => game.GoToMarket());
        if (result.success)
            SceneFlowController.Navigate(SceneNames.Market);
    }

    public void EnterCave()
    {
        if (session.Game.dayCycle.IsInHabitat)
        {
            session.Publish("Mağara açılıyor.");
            SceneFlowController.Navigate(SceneNames.Cave);
            return;
        }

        var result = session.RunAction(game => game.EnterHabitat(0));
        if (result.success)
            SceneFlowController.Navigate(SceneNames.Cave);
    }

    public void TravelDeeper()
    {
        if (!EnsureInCaveForHabitatAction())
            return;

        var result = session.RunAction(game => game.TravelDeeper());
        if (result.success)
            SceneFlowController.Navigate(SceneNames.Cave);
    }

    public void PlaceTrap()
    {
        if (!EnsureInCaveForHabitatAction())
            return;

        session.RunAction(game => game.PlaceTrap());
    }

    public void PlaceTrapWithRabbit()
    {
        if (!EnsureInCaveForHabitatAction())
            return;

        session.RunAction(game => game.PlaceTrapWithRabbit());
    }
    public void BuildSafeRoom()
    {
        if (!EnsureInCaveForHabitatAction())
            return;

        session.RunAction(game => game.BuildSafeRoom());
    }

    public void EndDay()
    {
        var result = session.RunAction(game => game.EndDay());
        if (result.success)
            RouteToCurrentLocation();
    }

    public void ReturnToMainHub()
    {
        session.Publish("Ana ekrana dönülüyor.");
        SceneFlowController.Navigate(SceneNames.MainHub);
    }

    public void ReturnToHeadquarters()
    {
        if (session.Game.dayCycle.IsInHabitat)
            session.RunAction(game => game.GoToMarket());

        session.Publish("Ana merkeze dönülüyor.");
        SceneFlowController.Navigate(SceneNames.MainHub);
    }

    public void BuyTrap()
    {
        if (!EnsureInMarket())
            return;

        session.RunAction(game => game.BuyTrap());
    }

    public void BuyRabbit()
    {
        if (!EnsureInMarket())
            return;

        session.RunAction(game => game.BuyRabbit());
    }

    public void SellFirstAliveDragon()
    {
        if (!EnsureInMarket())
            return;

        session.RunAction(game => game.SellFirstAliveDragon());
    }

    public void SellFirstDeadDragon()
    {
        if (!EnsureInMarket())
            return;

        session.RunAction(game => game.SellFirstDeadDragon());
    }

    public void SellDragon(string dragonName, bool alive)
    {
        if (!EnsureInMarket())
            return;

        session.RunAction(game => game.SellDragon(dragonName, alive));
    }

    bool EnsureInMarket()
    {
        if (session.Game.dayCycle.IsInMarket)
            return true;

        session.Publish("Önce markete gidin.");
        return false;
    }

    /// <summary>
    /// Oyuncu habitatta değilse önce EnterHabitat(0) + mağara UI'sine geçer.
    /// </summary>
    bool EnsureInCaveForHabitatAction()
    {
        if (session.Game.dayCycle.IsInHabitat)
            return true;

        session.Publish("Önce mağaraya giriliyor…");
        var result = session.RunAction(game => game.EnterHabitat(0));
        if (!result.success)
            return false;

        SceneFlowController.Navigate(SceneNames.Cave);
        return session.Game.dayCycle.IsInHabitat;
    }

    void RouteToCurrentLocation()
    {
        if (session.Game.dayCycle.IsInHabitat)
            SceneFlowController.Navigate(SceneNames.Cave);
        else
            SceneFlowController.Navigate(SceneNames.MainHub);
    }
}
