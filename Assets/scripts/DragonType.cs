using UnityEngine;

[System.Serializable]
public class DragonType
{
    [Tooltip("Dragon display name.")]
    public string name;

    [Tooltip("Rarity range: 1 common, 5 legendary.")]
    [Range(1, 5)] public int rarity = 1;

    [Tooltip("Flat modifier added to the capture chance.")]
    [Range(-0.3f, 0.3f)] public float catchMod;

    [Tooltip("Sale price when captured alive.")]
    public int alivePrice = 40;

    [Tooltip("Sale price when recovered dead.")]
    public int deadPrice = 10;

    public DragonType()
    {
    }

    public DragonType(string name, int rarity, float catchMod, int alivePrice, int deadPrice)
    {
        this.name = name;
        this.rarity = rarity;
        this.catchMod = catchMod;
        this.alivePrice = alivePrice;
        this.deadPrice = deadPrice;
    }

    public static DragonType[] CreateDefaultRoster()
    {
        return new DragonType[]
        {
            new DragonType("Kömürkanat", 1, 0.10f, 40, 10),
            new DragonType("Bakırdiş", 2, 0.05f, 60, 15),
            new DragonType("Buzsırt", 3, 0.00f, 90, 25),
            new DragonType("Gölgepul", 4, -0.10f, 140, 40),
            new DragonType("Altınkral", 5, -0.20f, 220, 70)
        };
    }
}