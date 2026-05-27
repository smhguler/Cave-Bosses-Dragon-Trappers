using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DragonCaptureResult
{
    public bool captured;
    public bool alive;
    public DragonType dragonType;
    public float captureChance;
    public string message;
}

[System.Serializable]
public class DragonCaptureSystem
{
    [Range(0f, 1f)] public float baitedCaptureChance = 0.65f;
    [Range(0f, 1f)] public float unbaitedCaptureChance = 0.05f;
    [Range(0f, 1f)] public float rabbitBaitCaptureBonus = 0.25f;
    [Range(0f, 1f)] public float aliveCaptureChance = 0.75f;
    [Range(0f, 3f)] public float rabbitBaitRarityBias = 1.50f;
    [Range(0f, 2f)] public float rabbitBaitPriceBias = 0.35f;

    public DragonCaptureResult TryCapture(Inventory inventory, HabitatModel habitat, DragonType[] dragonTypes, IGameRandom random)
    {
        return TryCapture(inventory, habitat, dragonTypes, random, false);
    }

    public DragonCaptureResult TryCapture(Inventory inventory, HabitatModel habitat, DragonType[] dragonTypes, IGameRandom random, bool useRabbitBait)
    {
        DragonType dragonType = RollDragonType(habitat, dragonTypes, random, useRabbitBait);
        if (dragonType == null)
        {
            return new DragonCaptureResult
            {
                captured = false,
                message = "Habitatta yakalanabilir ejderha turu yok."
            };
        }

        float baseChance = unbaitedCaptureChance;
        float baitBonus = useRabbitBait ? rabbitBaitCaptureBonus : 0f;
        float captureChance = Mathf.Clamp01(baseChance + baitBonus + dragonType.catchMod + habitat.captureModifier);

        if (random.Value() >= captureChance)
        {
            return new DragonCaptureResult
            {
                captured = false,
                dragonType = dragonType,
                captureChance = captureChance,
                message = string.Format("{0} tuzaktan kacti. Sans: %{1:0}", dragonType.name, captureChance * 100f)
            };
        }

        bool alive = random.Value() < aliveCaptureChance;
        return new DragonCaptureResult
        {
            captured = true,
            alive = alive,
            dragonType = dragonType,
            captureChance = captureChance,
            message = string.Format("{0} {1} yakalandi. Sans: %{2:0}", alive ? "Canli" : "Olu", dragonType.name, captureChance * 100f)
        };
    }

    public DragonType RollDragonType(HabitatModel habitat, DragonType[] dragonTypes, IGameRandom random)
    {
        return RollDragonType(habitat, dragonTypes, random, false);
    }

    public DragonType RollDragonType(HabitatModel habitat, DragonType[] dragonTypes, IGameRandom random, bool useRabbitBait)
    {
        if (dragonTypes == null || dragonTypes.Length == 0)
        {
            return null;
        }

        List<DragonType> candidates = new List<DragonType>();
        for (int i = 0; i < dragonTypes.Length; i++)
        {
            DragonType dragonType = dragonTypes[i];
            if (dragonType == null)
            {
                continue;
            }

            if (habitat == null || habitat.AllowsRarity(dragonType.rarity))
            {
                candidates.Add(dragonType);
            }
        }

        if (candidates.Count == 0)
        {
            for (int i = 0; i < dragonTypes.Length; i++)
            {
                if (dragonTypes[i] != null)
                {
                    candidates.Add(dragonTypes[i]);
                }
            }
        }

        float totalWeight = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += WeightFor(candidates[i], useRabbitBait);
        }

        float roll = random.Value() * totalWeight;
        for (int i = 0; i < candidates.Count; i++)
        {
            roll -= WeightFor(candidates[i], useRabbitBait);
            if (roll <= 0f)
            {
                return candidates[i];
            }
        }

        return candidates[0];
    }

    private float WeightFor(DragonType dragonType, bool useRabbitBait)
    {
        int rarity = Mathf.Max(1, dragonType.rarity);
        float weight = 1f / (rarity * rarity);
        if (!useRabbitBait)
        {
            return weight;
        }

        float rarity01 = Mathf.Clamp01((rarity - 1f) / 4f);
        float price01 = Mathf.Clamp01(dragonType.alivePrice / 220f);
        float rarityMultiplier = Mathf.Lerp(0.75f, 0.75f + rabbitBaitRarityBias, rarity01);
        float priceMultiplier = 1f + price01 * rabbitBaitPriceBias;
        return weight * rarityMultiplier * priceMultiplier;
    }
}