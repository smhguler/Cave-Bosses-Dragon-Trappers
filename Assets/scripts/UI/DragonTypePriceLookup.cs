/// <summary>
/// UI-only: dragonTypes roster üzerinden satış fiyatı çözümü.
/// </summary>
public static class DragonTypePriceLookup
{
    public static (int alivePrice, int deadPrice) Resolve(DragonType[] dragonTypes, string dragonName)
    {
        if (dragonTypes == null || string.IsNullOrEmpty(dragonName))
            return (0, 0);

        for (var i = 0; i < dragonTypes.Length; i++)
        {
            var type = dragonTypes[i];
            if (type == null || type.name != dragonName)
                continue;

            return (type.alivePrice, type.deadPrice);
        }

        return (0, 0);
    }
}
