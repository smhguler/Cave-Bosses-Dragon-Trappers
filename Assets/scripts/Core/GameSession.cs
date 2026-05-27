using System;
using UnityEngine;

/// <summary>
/// Runtime session bridge for the prototype UI. The gameplay source of truth is PrototypeGameLoop.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Prototype Loop")]
    public PrototypeGameLoop Game = new PrototypeGameLoop();

    [Header("Deterministic Testing")]
    public bool useDeterministicSeed;
    public int randomSeed = 1337;

    [Header("Read Only Debug")]
    [TextArea(6, 18)] public string lastReport;

    IGameActionsService actions;

    public IGameActionsService Actions => actions ??= new GameActionsService(this);

    public int Day => Game == null ? 1 : Game.dayCycle.day;
    public float TimeLeft => Game == null ? 0f : Game.timeSystem.timeLeft;
    public int Gold => Game == null ? 0 : Game.inventory.gold;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRun();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void InitializeRun()
    {
        EnsureGameLoop();
        Game.Initialize(useDeterministicSeed, randomSeed);
        RefreshReport();
    }

    public GameActionResult RunAction(Func<PrototypeGameLoop, GameActionResult> action)
    {
        EnsureGameLoop();

        var result = action == null
            ? GameActionResult.Failure("Oyun aksiyonu bulunamadı.")
            : action(Game);

        RefreshReport();
        Publish(result.message);
        return result;
    }

    public string RefreshReport()
    {
        EnsureGameLoop();
        lastReport = Game.BuildStatusText();
        return lastReport;
    }

    public void Publish(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        Debug.Log($"[Game] {message}");
        UiFeedbackLog.Publish(message);
    }

    void EnsureGameLoop()
    {
        if (Game == null)
            Game = new PrototypeGameLoop();
    }
}
