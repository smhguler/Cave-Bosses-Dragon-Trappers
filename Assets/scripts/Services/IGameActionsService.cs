/// <summary>
/// Ana ekran ve mağara aksiyonları için oyun sistemi sözleşmesi.
/// UI yalnızca bu arayüzü çağırır; gerçek simülasyon ileride genişletilir.
/// </summary>
public interface IGameActionsService
{
    void GoToMarket();
    void EnterCave();
    void TravelDeeper();
    void PlaceTrap();
    void PlaceTrapWithRabbit();
    void BuildSafeRoom();
    void EndDay();
    void ReturnToMainHub();
    void ReturnToHeadquarters();
    void BuyTrap();
    void BuyRabbit();
    void SellFirstAliveDragon();
    void SellFirstDeadDragon();
    void SellDragon(string dragonName, bool alive);
}
