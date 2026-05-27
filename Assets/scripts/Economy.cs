using UnityEngine;

[System.Serializable]
public class Economy
{
    [Header("Market Timing")]
    [Min(0f)] public float marketActionSeconds = 4f;

    [Header("Starting Stock")]
    public int startingVendorTraps = 5;
    public int startingVendorRabbits = 3;

    [Header("Vendor Stock")]
    public int vendorTraps = 5;
    public int vendorRabbits = 3;

    [Header("Vendor Targets")]
    public int vendorTrapTarget = 6;
    public int vendorRabbitTarget = 4;

    [Header("Vendor Base Prices")]
    public int trapBaseBuy = 20;
    public int rabbitBaseBuy = 10;

    [Header("Vendor Prices")]
    public int trapBuyPrice;
    public int trapSellPrice;
    public int rabbitBuyPrice;
    public int rabbitSellPrice;

    public void ResetForNewRun(int currentDay)
    {
        vendorTraps = startingVendorTraps;
        vendorRabbits = startingVendorRabbits;
        RecalculatePrices(currentDay);
    }

    public void NewDay(int currentDay, IGameRandom random)
    {
        vendorTraps = Mathf.Clamp(vendorTraps + random.Range(1, 4), 0, vendorTrapTarget);
        vendorRabbits = Mathf.Clamp(vendorRabbits + random.Range(1, 3), 0, vendorRabbitTarget);
        RecalculatePrices(currentDay);
    }

    public GameActionResult BuyTrap(Inventory inventory, TimeSystem timeSystem, int currentDay)
    {
        if (vendorTraps <= 0)
        {
            return GameActionResult.Failure("Satıcıda tuzak kalmadı.");
        }

        if (inventory.gold < trapBuyPrice)
        {
            return GameActionResult.Failure(string.Format("Altın yetmiyor. Tuzak fiyatı: {0}", trapBuyPrice));
        }

        string timeMessage;
        if (!timeSystem.TrySpend(marketActionSeconds, "Tuzak satın alma", out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.SpendGold(trapBuyPrice);
        inventory.AddTraps(1);
        vendorTraps--;
        RecalculatePrices(currentDay);
        return GameActionResult.Success(string.Format("{0}\n1 tuzak aldın. Altın -{1}", timeMessage, trapBuyPrice));
    }

    public GameActionResult BuyRabbit(Inventory inventory, TimeSystem timeSystem, int currentDay)
    {
        if (vendorRabbits <= 0)
        {
            return GameActionResult.Failure("Satıcıda tavşan kalmadı.");
        }

        if (inventory.gold < rabbitBuyPrice)
        {
            return GameActionResult.Failure(string.Format("Altın yetmiyor. Tavşan fiyatı: {0}", rabbitBuyPrice));
        }

        string timeMessage;
        if (!timeSystem.TrySpend(marketActionSeconds, "Tavşan satın alma", out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.SpendGold(rabbitBuyPrice);
        inventory.AddRabbits(1);
        vendorRabbits--;
        RecalculatePrices(currentDay);
        return GameActionResult.Success(string.Format("{0}\n1 tavşan aldın. Altın -{1}", timeMessage, rabbitBuyPrice));
    }

    public GameActionResult SellTrap(Inventory inventory, TimeSystem timeSystem, int currentDay)
    {
        if (inventory.traps <= 0)
        {
            return GameActionResult.Failure("Satacak tuzağın yok.");
        }

        string timeMessage;
        if (!timeSystem.TrySpend(marketActionSeconds, "Tuzak satışı", out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.ConsumeTrap(1);
        inventory.AddGold(trapSellPrice);
        vendorTraps++;
        RecalculatePrices(currentDay);
        return GameActionResult.Success(string.Format("{0}\n1 tuzak sattın. Altın +{1}", timeMessage, trapSellPrice));
    }

    public GameActionResult SellRabbit(Inventory inventory, TimeSystem timeSystem, int currentDay)
    {
        if (inventory.rabbits <= 0)
        {
            return GameActionResult.Failure("Satacak tavşanın yok.");
        }

        string timeMessage;
        if (!timeSystem.TrySpend(marketActionSeconds, "Tavşan satışı", out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.ConsumeRabbit(1);
        inventory.AddGold(rabbitSellPrice);
        vendorRabbits++;
        RecalculatePrices(currentDay);
        return GameActionResult.Success(string.Format("{0}\n1 tavşan sattın. Altın +{1}", timeMessage, rabbitSellPrice));
    }

    public GameActionResult SellDragon(Inventory inventory, TimeSystem timeSystem, DragonType dragonType, bool alive)
    {
        if (dragonType == null)
        {
            return GameActionResult.Failure("Satılacak ejderha türü bulunamadı.");
        }

        if (inventory.GetDragonCount(dragonType.name, alive) <= 0)
        {
            return GameActionResult.Failure("Bu türden satılacak ejderha yok.");
        }

        string timeMessage;
        if (!timeSystem.TrySpend(marketActionSeconds, "Ejderha satışı", out timeMessage))
        {
            return GameActionResult.Failure(timeMessage);
        }

        inventory.RemoveDragon(dragonType.name, alive);
        int price = alive ? dragonType.alivePrice : dragonType.deadPrice;
        inventory.AddGold(price);
        return GameActionResult.Success(string.Format("{0}\n{1} {2} sattın. Altın +{3}", timeMessage, alive ? "Canlı" : "Ölü", dragonType.name, price));
    }

    public void RecalculatePrices(int currentDay)
    {
        trapBuyPrice = CalcBuyPrice(trapBaseBuy, vendorTraps, vendorTrapTarget, currentDay);
        rabbitBuyPrice = CalcBuyPrice(rabbitBaseBuy, vendorRabbits, vendorRabbitTarget, currentDay);
        trapSellPrice = Mathf.Max(1, Mathf.RoundToInt(trapBuyPrice * 0.6f));
        rabbitSellPrice = Mathf.Max(1, Mathf.RoundToInt(rabbitBuyPrice * 0.6f));
    }

    public int CalcBuyPrice(int basePrice, int stock, int target, int currentDay)
    {
        float ratio = target <= 0 ? 1f : Mathf.Clamp01(stock / (float)target);
        float multiplier = Mathf.Lerp(1.6f, 0.8f, ratio);
        float inflation = 1f + Mathf.Max(0, currentDay - 1) * 0.01f;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * multiplier * inflation));
    }
}