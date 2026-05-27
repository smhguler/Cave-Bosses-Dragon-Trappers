using UnityEngine;

public class PrototypeGame : MonoBehaviour
{
    [Header("Prototype Loop")]
    public PrototypeGameLoop game = new PrototypeGameLoop();

    [Header("Deterministic Testing")]
    public bool useDeterministicSeed = true;
    public int randomSeed = 1337;
    public bool runSmokeSimulationOnStart;

    [Header("Read Only Debug")]
    [TextArea(8, 24)] public string lastReport;

    private void Start()
    {
        InitializeNewRun();

        if (runSmokeSimulationOnStart)
        {
            RunSmokeSimulation();
        }
    }

    [ContextMenu("Initialize New Run")]
    public void InitializeNewRun()
    {
        game.Initialize(useDeterministicSeed, randomSeed);
        RefreshReport();
    }

    [ContextMenu("Buy Trap")]
    public void BuyTrap()
    {
        game.BuyTrap();
        RefreshReport();
    }

    [ContextMenu("Buy Rabbit")]
    public void BuyRabbit()
    {
        game.BuyRabbit();
        RefreshReport();
    }

    [ContextMenu("Enter First Habitat")]
    public void EnterFirstHabitat()
    {
        game.EnterHabitat(0);
        RefreshReport();
    }

    [ContextMenu("Travel Deeper")]
    public void TravelDeeper()
    {
        game.TravelDeeper();
        RefreshReport();
    }

    [ContextMenu("Place Trap")]
    public void PlaceTrap()
    {
        game.PlaceTrap();
        RefreshReport();
    }

    [ContextMenu("Build Safe Room")]
    public void BuildSafeRoom()
    {
        game.BuildSafeRoom();
        RefreshReport();
    }

    [ContextMenu("Go To Market")]
    public void GoToMarket()
    {
        game.GoToMarket();
        RefreshReport();
    }

    [ContextMenu("Sell First Alive Dragon")]
    public void SellFirstAliveDragon()
    {
        game.SellFirstAliveDragon();
        RefreshReport();
    }

    [ContextMenu("Sell First Dead Dragon")]
    public void SellFirstDeadDragon()
    {
        game.SellFirstDeadDragon();
        RefreshReport();
    }

    [ContextMenu("End Day")]
    public void EndDay()
    {
        game.EndDay();
        RefreshReport();
    }

    [ContextMenu("Run Smoke Simulation")]
    public void RunSmokeSimulation()
    {
        game.Initialize(useDeterministicSeed, randomSeed);
        game.BuyRabbit();
        game.BuyTrap();
        game.EnterHabitat(0);
        game.PlaceTrap();
        game.PlaceTrap();
        game.TravelDeeper();
        game.BuildSafeRoom();
        game.EndDay();
        game.GoToMarket();
        game.SellFirstAliveDragon();
        game.SellFirstDeadDragon();
        RefreshReport();
    }

    public string RefreshReport()
    {
        lastReport = game.BuildStatusText();
        Debug.Log(lastReport);
        return lastReport;
    }
}