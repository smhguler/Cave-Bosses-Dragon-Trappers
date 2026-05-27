using UnityEngine;

public enum TrapOutcome
{
    None,
    Rabbit,
    DragonCaptured,
    DragonEscaped,
    Empty
}

public enum TrapKind
{
    Simple,
    RabbitBait
}

[System.Serializable]
public class TrapSystem
{
    [Range(0f, 1f)] public float rabbitBaitDragonEncounterBonus = 0.20f;
    [Min(0f)] public float rabbitBaitExtraSeconds = 8f;

    public TrapOutcome lastOutcome = TrapOutcome.None;
    public TrapKind lastTrapKind = TrapKind.Simple;

    public GameActionResult PlaceTrap(
        Inventory inventory,
        TimeSystem timeSystem,
        HabitatModel habitat,
        DragonCaptureSystem captureSystem,
        DragonType[] dragonTypes,
        IGameRandom random)
    {
        return PlaceTrapInternal(inventory, timeSystem, habitat, captureSystem, dragonTypes, random, false);
    }

    public GameActionResult PlaceTrapWithRabbit(
        Inventory inventory,
        TimeSystem timeSystem,
        HabitatModel habitat,
        DragonCaptureSystem captureSystem,
        DragonType[] dragonTypes,
        IGameRandom random)
    {
        return PlaceTrapInternal(inventory, timeSystem, habitat, captureSystem, dragonTypes, random, true);
    }

    private GameActionResult PlaceTrapInternal(
        Inventory inventory,
        TimeSystem timeSystem,
        HabitatModel habitat,
        DragonCaptureSystem captureSystem,
        DragonType[] dragonTypes,
        IGameRandom random,
        bool useRabbitBait)
    {
        lastTrapKind = useRabbitBait ? TrapKind.RabbitBait : TrapKind.Simple;

        if (habitat == null)
        {
            return GameActionResult.Failure("Tuzak kurmak icin aktif habitat yok.");
        }

        if (inventory.traps <= 0)
        {
            return GameActionResult.Failure("Kuracak tuzagin yok.");
        }

        if (useRabbitBait && inventory.rabbits <= 0)
        {
            return GameActionResult.Failure("Tavsan yemli tuzak icin tavsan yok.");
        }

        string timeMessage;
        string timeReason = useRabbitBait ? "Tavsan yemli tuzak kurma" : "Tuzak kurma";
        float trapPlacementSeconds = habitat.trapPlacementSeconds + (useRabbitBait ? rabbitBaitExtraSeconds : 0f);
        if (!timeSystem.TrySpend(trapPlacementSeconds, timeReason, out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.ConsumeTrap(1);
        if (useRabbitBait)
        {
            inventory.ConsumeRabbit(1);
        }

        float roll = random.Value();
        string trapLabel = useRabbitBait ? "Tavsan yemli tuzak" : "Tuzak";

        if (!useRabbitBait && roll < habitat.rabbitChance)
        {
            inventory.AddRabbits(1);
            lastOutcome = TrapOutcome.Rabbit;
            return GameActionResult.Success(timeMessage + "\nTuzak tavsan yakaladi.");
        }

        float dragonThreshold = useRabbitBait
            ? Mathf.Clamp01(habitat.dragonEncounterChance + rabbitBaitDragonEncounterBonus)
            : Mathf.Clamp01(habitat.rabbitChance + habitat.dragonEncounterChance);
        if (roll > dragonThreshold)
        {
            lastOutcome = TrapOutcome.Empty;
            return GameActionResult.Success(timeMessage + "\n" + trapLabel + " bos kaldi.");
        }

        DragonCaptureResult capture = captureSystem.TryCapture(inventory, habitat, dragonTypes, random, useRabbitBait);
        if (capture.captured)
        {
            inventory.AddDragon(capture.dragonType.name, capture.alive);
            lastOutcome = TrapOutcome.DragonCaptured;
        }
        else
        {
            lastOutcome = TrapOutcome.DragonEscaped;
        }

        return GameActionResult.Success(timeMessage + "\n" + trapLabel + ": " + capture.message);
    }
}
