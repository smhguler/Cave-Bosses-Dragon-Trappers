using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HabitatModel
{
    public string id = "outer-cave";
    public string displayName = "Dış Mağara";

    [Min(1)] public int depth = 1;
    [Range(1, 5)] public int minRarity = 1;
    [Range(1, 5)] public int maxRarity = 2;

    [Header("Encounter")]
    [Range(0f, 1f)] public float rabbitChance = 0.35f;
    [Range(0f, 1f)] public float dragonEncounterChance = 0.60f;
    [Range(-0.3f, 0.3f)] public float captureModifier;
    [Range(0f, 1f)] public float danger = 0.10f;

    [Header("Time Costs")]
    [Min(0f)] public float enterSeconds = 25f;
    [Min(0f)] public float exitSeconds = 15f;
    [Min(0f)] public float travelDeeperSeconds = 20f;
    [Min(0f)] public float trapPlacementSeconds = 12f;
    [Min(0f)] public float safeRoomBuildSeconds = 40f;

    [Header("Safe Room Costs")]
    [Min(0)] public int safeRoomGoldCost;
    [Min(0)] public int safeRoomTrapCost;

    [Header("Clear Requirements")]
    [Min(0)] public int requiredCaptures = 1;
    [Min(0)] public int requiredScore = 2;

    public bool AllowsRarity(int rarity)
    {
        return rarity >= minRarity && rarity <= maxRarity;
    }

    public bool IsClearedBy(int captures, int score)
    {
        bool captureGoalMet = requiredCaptures > 0 && captures >= requiredCaptures;
        bool scoreGoalMet = requiredScore > 0 && score >= requiredScore;
        return captureGoalMet || scoreGoalMet;
    }

    public static HabitatModel Create(
        string id,
        string displayName,
        int depth,
        int minRarity,
        int maxRarity,
        float rabbitChance,
        float dragonEncounterChance,
        float captureModifier,
        float danger,
        int safeRoomGoldCost = 0,
        int safeRoomTrapCost = 0,
        int requiredCaptures = 1,
        int requiredScore = 2)
    {
        return new HabitatModel
        {
            id = id,
            displayName = displayName,
            depth = depth,
            minRarity = minRarity,
            maxRarity = maxRarity,
            rabbitChance = rabbitChance,
            dragonEncounterChance = dragonEncounterChance,
            captureModifier = captureModifier,
            danger = danger,
            safeRoomGoldCost = safeRoomGoldCost,
            safeRoomTrapCost = safeRoomTrapCost,
            requiredCaptures = requiredCaptures,
            requiredScore = requiredScore
        };
    }
}

[System.Serializable]
public class HabitatCatalog
{
    public List<HabitatModel> habitats = new List<HabitatModel>();

    public int Count
    {
        get { return habitats == null ? 0 : habitats.Count; }
    }

    public void EnsureDefaults()
    {
        if (habitats == null)
        {
            habitats = new List<HabitatModel>();
        }

        if (habitats.Count > 0)
        {
            return;
        }

        habitats.Add(HabitatModel.Create("outer-cave", "Dış Mağara", 1, 1, 2, 0.38f, 0.55f, 0.05f, 0.10f, 30, 0, 1, 2));
        habitats.Add(HabitatModel.Create("crystal-tunnels", "Kristal Tüneller", 2, 1, 3, 0.30f, 0.65f, 0.00f, 0.20f, 40, 1, 2, 3));
        habitats.Add(HabitatModel.Create("ash-chasm", "Kül Yarığı", 3, 2, 4, 0.22f, 0.75f, -0.05f, 0.35f, 60, 2, 2, 4));
        habitats.Add(HabitatModel.Create("ancient-nest", "Kadim Yuva", 4, 3, 5, 0.12f, 0.85f, -0.10f, 0.55f, 90, 3, 3, 5));
    }

    public HabitatModel Get(int index)
    {
        EnsureDefaults();

        if (index < 0 || index >= habitats.Count)
        {
            return null;
        }

        return habitats[index];
    }
}
