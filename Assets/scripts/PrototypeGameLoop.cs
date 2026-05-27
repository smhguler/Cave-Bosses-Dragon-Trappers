using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class PrototypeGameLoop
{
    [Header("Core Systems")]
    public DayCycle dayCycle = new DayCycle();
    public TimeSystem timeSystem = new TimeSystem();
    public Inventory inventory = new Inventory();
    public Economy economy = new Economy();
    public Scoring scoring = new Scoring();
    public HabitatCatalog habitats = new HabitatCatalog();
    public TrapSystem trapSystem = new TrapSystem();
    public DragonCaptureSystem dragonCaptureSystem = new DragonCaptureSystem();

    [Header("Content")]
    public DragonType[] dragonTypes = DragonType.CreateDefaultRoster();

    [Header("Runtime Log")]
    public List<string> eventLog = new List<string>();

    [Header("Run Target")]
    public RunTargetState runState = RunTargetState.Running;

    [Header("Day Report")]
    public DayReport currentDayReport = new DayReport();
    public DayReport lastCompletedDayReport;

    private IGameRandom random = new UnityGameRandom();
    private bool initialized;

    public void Initialize(bool deterministicRandom, int seed)
    {
        random = deterministicRandom ? (IGameRandom)new SeededGameRandom(seed) : new UnityGameRandom();
        EnsureContent();

        dayCycle.ResetRun();
        timeSystem.InitializeForNewRun();
        inventory.ResetForNewRun(dragonTypes);
        economy.ResetForNewRun(dayCycle.day);
        scoring.Recalculate(inventory, dayCycle);
        runState = RunTargetState.Running;
        lastCompletedDayReport = null;
        StartDayReport();

        eventLog.Clear();
        initialized = true;
        Log("Yeni koşu başladı.");
    }

    public GameActionResult BuyTrap()
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Tuzak sadece pazarda satın alınır."));
        }

        int goldBefore = inventory.gold;
        GameActionResult result = economy.BuyTrap(inventory, timeSystem, dayCycle.day);
        if (result.success)
        {
            EnsureCurrentDayReport();
            currentDayReport.trapsBought++;
            currentDayReport.AddExpense(goldBefore - inventory.gold);
        }

        return Complete(result);
    }

    public GameActionResult BuyRabbit()
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Tavşan sadece pazarda satın alınır."));
        }

        int goldBefore = inventory.gold;
        GameActionResult result = economy.BuyRabbit(inventory, timeSystem, dayCycle.day);
        if (result.success)
        {
            EnsureCurrentDayReport();
            currentDayReport.rabbitsBought++;
            currentDayReport.AddExpense(goldBefore - inventory.gold);
        }

        return Complete(result);
    }

    public GameActionResult SellTrap()
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Satış sadece pazarda yapılır."));
        }

        int goldBefore = inventory.gold;
        GameActionResult result = economy.SellTrap(inventory, timeSystem, dayCycle.day);
        if (result.success)
        {
            EnsureCurrentDayReport();
            currentDayReport.AddIncome(inventory.gold - goldBefore);
        }

        return Complete(result);
    }

    public GameActionResult SellRabbit()
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Satış sadece pazarda yapılır."));
        }

        int goldBefore = inventory.gold;
        GameActionResult result = economy.SellRabbit(inventory, timeSystem, dayCycle.day);
        if (result.success)
        {
            EnsureCurrentDayReport();
            currentDayReport.AddIncome(inventory.gold - goldBefore);
        }

        return Complete(result);
    }

    public GameActionResult SellFirstAliveDragon()
    {
        string dragonName = inventory.FindFirstDragonName(true);
        return SellDragon(dragonName, true);
    }

    public GameActionResult SellFirstDeadDragon()
    {
        string dragonName = inventory.FindFirstDragonName(false);
        return SellDragon(dragonName, false);
    }

    public GameActionResult SellDragon(string dragonName, bool alive)
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Ejderha satışı sadece pazarda yapılır."));
        }

        DragonType dragonType = FindDragonType(dragonName);
        int goldBefore = inventory.gold;
        GameActionResult result = economy.SellDragon(inventory, timeSystem, dragonType, alive);
        if (result.success)
        {
            EnsureCurrentDayReport();
            currentDayReport.AddIncome(inventory.gold - goldBefore);
            if (alive)
            {
                currentDayReport.liveDragonsSold++;
            }
            else
            {
                currentDayReport.deadDragonsSold++;
            }
        }

        return Complete(result);
    }

    public GameActionResult EnterHabitat(int habitatIndex)
    {
        EnsureInitialized();
        if (!dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Yeni habitata pazardan gidilir."));
        }

        HabitatModel habitat = habitats.Get(habitatIndex);
        if (habitat == null)
        {
            return Complete(GameActionResult.Failure("Habitat bulunamadı."));
        }

        string timeMessage;
        if (!timeSystem.TrySpend(habitat.enterSeconds, "Habitata yolculuk", out timeMessage))
        {
            return Complete(GameActionResult.Failure(timeMessage));
        }

        dayCycle.EnterHabitat(habitat, habitatIndex);
        runState = dayCycle.IsHabitatCleared(habitatIndex) ? RunTargetState.HabitatCleared : RunTargetState.Running;
        return Complete(GameActionResult.Success(string.Format("{0}\n{1} habitatına girdin.", timeMessage, habitat.displayName)));
    }

    public GameActionResult TravelDeeper()
    {
        EnsureInitialized();
        if (!dayCycle.IsInHabitat)
        {
            return Complete(GameActionResult.Failure("Daha derine sadece habitattayken gidilir."));
        }

        int nextIndex = dayCycle.currentHabitatIndex + 1;
        HabitatModel nextHabitat = habitats.Get(nextIndex);
        if (nextHabitat == null)
        {
            string reason = dayCycle.AreAllHabitatsCleared(habitats.Count)
                ? "Daha derin habitat yok; tum magara zaten tamamlandi."
                : "Daha derin habitat yok; mevcut katalog son katmanda.";
            return Complete(GameActionResult.Failure(reason));
        }

        string timeMessage;
        if (!timeSystem.TrySpend(dayCycle.currentHabitat.travelDeeperSeconds, "Daha derine inme", out timeMessage))
        {
            return Complete(GameActionResult.Failure(timeMessage));
        }

        dayCycle.EnterHabitat(nextHabitat, nextIndex);
        runState = dayCycle.IsHabitatCleared(nextIndex) ? RunTargetState.HabitatCleared : RunTargetState.Running;
        return Complete(GameActionResult.Success(string.Format("{0}\n{1} habitatına indin.", timeMessage, nextHabitat.displayName)));
    }

    public GameActionResult GoToMarket()
    {
        EnsureInitialized();
        if (dayCycle.IsInMarket)
        {
            return Complete(GameActionResult.Failure("Zaten pazardasın."));
        }

        HabitatModel habitat = dayCycle.currentHabitat;
        float exitSeconds = habitat == null ? 15f : habitat.exitSeconds;

        string timeMessage;
        if (!timeSystem.TrySpend(exitSeconds, "Pazara dönüş", out timeMessage))
        {
            return Complete(GameActionResult.Failure(timeMessage));
        }

        dayCycle.GoToMarket();
        return Complete(GameActionResult.Success(timeMessage + "\nPazara döndün."));
    }

    public GameActionResult BuildSafeRoom()
    {
        EnsureInitialized();
        if (!dayCycle.IsInHabitat)
        {
            return Complete(GameActionResult.Failure("Güvenli oda sadece habitattayken kurulabilir."));
        }

        if (dayCycle.hasSafeRoom)
        {
            return Complete(GameActionResult.Failure("Bu habitatta zaten güvenli oda kurulu."));
        }

        HabitatModel habitat = dayCycle.currentHabitat;
        if (inventory.gold < habitat.safeRoomGoldCost && inventory.traps < habitat.safeRoomTrapCost)
        {
            return Complete(GameActionResult.Failure(string.Format(
                "Guvenli oda kurulamadi: altin ve tuzak malzemesi yetmiyor. Gerekli altin: {0}, mevcut: {1}. Gerekli tuzak: {2}, mevcut: {3}.",
                habitat.safeRoomGoldCost,
                inventory.gold,
                habitat.safeRoomTrapCost,
                inventory.traps)));
        }

        if (inventory.gold < habitat.safeRoomGoldCost)
        {
            return Complete(GameActionResult.Failure(string.Format(
                "Guvenli oda kurulamadi: altin yetmiyor. Gerekli: {0}, mevcut: {1}.",
                habitat.safeRoomGoldCost,
                inventory.gold)));
        }

        if (inventory.traps < habitat.safeRoomTrapCost)
        {
            return Complete(GameActionResult.Failure(string.Format(
                "Guvenli oda kurulamadi: tuzak malzemesi yetmiyor. Gerekli: {0}, mevcut: {1}.",
                habitat.safeRoomTrapCost,
                inventory.traps)));
        }

        string timeMessage;
        if (!timeSystem.TrySpend(habitat.safeRoomBuildSeconds, "Güvenli oda kurma", out timeMessage))
        {
            return Complete(GameActionResult.Failure(timeMessage));
        }

        int goldBefore = inventory.gold;
        int trapsBefore = inventory.traps;
        inventory.SpendGold(habitat.safeRoomGoldCost);
        inventory.ConsumeTrap(habitat.safeRoomTrapCost);
        dayCycle.BuildSafeRoom();
        EnsureCurrentDayReport();
        currentDayReport.safeRoomsBuilt++;
        int safeRoomTrapExpense = trapsBefore - inventory.traps;
        currentDayReport.safeRoomTrapsSpent += safeRoomTrapExpense;
        currentDayReport.AddExpense((goldBefore - inventory.gold) + safeRoomTrapExpense);
        return Complete(GameActionResult.Success(timeMessage + "\nGüvenli oda kuruldu. Geceyi burada atlatabilirsin."));
    }

    public GameActionResult PlaceTrap()
    {
        EnsureInitialized();
        if (!dayCycle.IsInHabitat)
        {
            return Complete(GameActionResult.Failure("Tuzak sadece habitattayken kurulur."));
        }

        int aliveBefore = inventory.dragonsAlive;
        int deadBefore = inventory.dragonsDead;

        GameActionResult result = trapSystem.PlaceTrap(
            inventory,
            timeSystem,
            dayCycle.currentHabitat,
            dragonCaptureSystem,
            dragonTypes,
            random);

        return CompleteTrapAction(result, aliveBefore, deadBefore, false);
    }

    public GameActionResult PlaceTrapWithRabbit()
    {
        EnsureInitialized();
        if (!dayCycle.IsInHabitat)
        {
            return Complete(GameActionResult.Failure("Tuzak sadece habitattayken kurulur."));
        }

        int aliveBefore = inventory.dragonsAlive;
        int deadBefore = inventory.dragonsDead;

        GameActionResult result = trapSystem.PlaceTrapWithRabbit(
            inventory,
            timeSystem,
            dayCycle.currentHabitat,
            dragonCaptureSystem,
            dragonTypes,
            random);

        return CompleteTrapAction(result, aliveBefore, deadBefore, true);
    }
    public GameActionResult Wait(float seconds)
    {
        EnsureInitialized();
        string timeMessage;
        if (!timeSystem.TrySpend(seconds, "Bekleme", out timeMessage))
        {
            return Complete(GameActionResult.Failure(timeMessage));
        }

        return Complete(GameActionResult.Success(timeMessage + "\nBiraz bekledin."));
    }

    public GameActionResult EndDay()
    {
        EnsureInitialized();
        GameActionResult result = ResolveNight("Günü bitirdin.");
        Log(result.message);
        Stamp(result);
        return result;
    }

    public DayReport GetLastDayReport()
    {
        return lastCompletedDayReport;
    }

    public string BuildStatusText()
    {
        EnsureInitialized();
        scoring.Recalculate(inventory, dayCycle);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("=== Dragon Trappers Prototype ===");
        builder.AppendLine(string.Format("Gün: {0}", dayCycle.day));
        builder.AppendLine(string.Format("Konum: {0}", dayCycle.location));
        builder.AppendLine(string.Format("Habitat: {0}", dayCycle.currentHabitat == null ? "-" : dayCycle.currentHabitat.displayName));
        builder.AppendLine(string.Format("Güvenli Oda: {0}", dayCycle.hasSafeRoom ? "Var" : "Yok"));
        builder.AppendLine(string.Format("Zaman: {0:0}/{1:0}", timeSystem.timeLeft, timeSystem.dayDuration));
        builder.AppendLine(string.Format("Altın: {0}", inventory.gold));
        builder.AppendLine(string.Format("Tuzak: {0}", inventory.traps));
        builder.AppendLine(string.Format("Tavşan: {0}", inventory.rabbits));
        builder.AppendLine(string.Format("Ejderha: Canlı {0} / Ölü {1}", inventory.dragonsAlive, inventory.dragonsDead));
        builder.AppendLine(string.Format("Skor: {0}", scoring.currentScore));
        builder.AppendLine(string.Format("Run Durumu: {0}", runState));
        builder.AppendLine(string.Format("Temizlenen Habitat: {0}/{1}", dayCycle.ClearedHabitatCount(), habitats.Count));
        if (currentDayReport != null)
        {
            builder.AppendLine(string.Format("Gun Raporu: Gelir {0} / Gider {1} / Net {2}",
                currentDayReport.totalIncome,
                currentDayReport.totalExpense,
                currentDayReport.netProfit));
            builder.AppendLine(string.Format("Gun Aksiyonlari: Tuzak {0}, Yemli {1}, Guvenli Oda {2}",
                currentDayReport.normalTrapsPlaced,
                currentDayReport.rabbitBaitTrapsPlaced,
                currentDayReport.safeRoomsBuilt));
        }
        if (dayCycle.currentHabitat != null)
        {
            builder.AppendLine(string.Format("Habitat Hedefi: Yakalama {0}/{1}, Skor {2}/{3}",
                dayCycle.CurrentHabitatCaptures,
                dayCycle.currentHabitat.requiredCaptures,
                dayCycle.CurrentHabitatScore,
                dayCycle.currentHabitat.requiredScore));
        }
        builder.AppendLine(string.Format("Satıcı: Tuzak {0} (Al {1}/Sat {2}), Tavşan {3} (Al {4}/Sat {5})",
            economy.vendorTraps,
            economy.trapBuyPrice,
            economy.trapSellPrice,
            economy.vendorRabbits,
            economy.rabbitBuyPrice,
            economy.rabbitSellPrice));

        builder.AppendLine("--- Ejderhalar ---");
        for (int i = 0; i < inventory.Dragons.Count; i++)
        {
            DragonInventoryEntry entry = inventory.Dragons[i];
            if (entry.alive > 0 || entry.dead > 0)
            {
                builder.AppendLine(string.Format("{0}: Canlı {1}, Ölü {2}", entry.dragonName, entry.alive, entry.dead));
            }
        }

        builder.AppendLine("--- Son Olaylar ---");
        int start = Mathf.Max(0, eventLog.Count - 8);
        for (int i = start; i < eventLog.Count; i++)
        {
            builder.AppendLine(eventLog[i]);
        }

        return builder.ToString();
    }

    public static void RunSmokeTest()
    {
        PrototypeGameLoop caveLoop = new PrototypeGameLoop();
        caveLoop.Initialize(true, 1337);

        Require(caveLoop.runState == RunTargetState.Running, "new run should start in running state");
        Require(caveLoop.habitats.Count >= 3, "default habitat catalog should expose at least 3 cave layers");
        Require(caveLoop.habitats.Get(0).safeRoomGoldCost == 30 && caveLoop.habitats.Get(0).safeRoomTrapCost == 0, "outer cave safe room default cost should match balance");
        Require(caveLoop.habitats.Get(1).safeRoomGoldCost == 40 && caveLoop.habitats.Get(1).safeRoomTrapCost == 1, "crystal tunnels safe room default cost should match balance");
        Require(caveLoop.habitats.Get(2).safeRoomGoldCost == 60 && caveLoop.habitats.Get(2).safeRoomTrapCost == 2, "ash chasm safe room default cost should match balance");
        Require(caveLoop.habitats.Get(3).safeRoomGoldCost == 90 && caveLoop.habitats.Get(3).safeRoomTrapCost == 3, "ancient nest safe room default cost should match balance");
        Require(caveLoop.EnterHabitat(0).success, "cave scenario should enter the first habitat");
        Require(caveLoop.dayCycle.IsInHabitat, "cave scenario should be in habitat after enter");
        Require(caveLoop.PlaceTrap().success, "cave scenario trap placement should resolve");

        GameActionResult unsafeNight = caveLoop.EndDay();
        Require(unsafeNight.success && unsafeNight.runReset, "ending the day in cave without safe room should reset the run");
        Require(unsafeNight.runState == RunTargetState.RunFailedReset, "unsafe cave night should stamp run failed/reset state");
        Require(MessageContains(unsafeNight, "Guvenli oda yoktu"), "unsafe cave night should explain missing safe room");
        Require(caveLoop.dayCycle.IsInMarket && caveLoop.dayCycle.day == 1, "unsafe cave night should return to day 1 market");
        DayReport unsafeReport = caveLoop.GetLastDayReport();
        Require(unsafeReport != null && unsafeReport.finalized, "unsafe EndDay should finalize a day report");
        Require(unsafeReport.runResetAtDayEnd, "unsafe EndDay should mark run reset in the day report");

        PrototypeGameLoop unsafeScoreLoop = new PrototypeGameLoop();
        unsafeScoreLoop.Initialize(true, 1337);
        string unsafeScoreDragonName = unsafeScoreLoop.dragonTypes[0].name;
        unsafeScoreLoop.inventory.AddDragon(unsafeScoreDragonName, true);
        int expectedUnsafeEndingScore = unsafeScoreLoop.scoring.aliveDragonPoints;
        Require(unsafeScoreLoop.EnterHabitat(0).success, "unsafe score report test should enter habitat");
        GameActionResult unsafeScoreNight = unsafeScoreLoop.EndDay();
        Require(unsafeScoreNight.success && unsafeScoreNight.runReset, "unsafe score report test should reset");
        DayReport unsafeScoreReport = unsafeScoreLoop.GetLastDayReport();
        Require(unsafeScoreReport != null && unsafeScoreReport.endingScore == expectedUnsafeEndingScore, "unsafe EndDay report should preserve ending score before reset");
        Require(unsafeScoreLoop.scoring.currentScore == 0, "unsafe reset should clear current score after report finalization");
        Require(unsafeScoreLoop.GetLastDayReport().endingScore == expectedUnsafeEndingScore, "GetLastDayReport should keep historical score after reset");

        PrototypeGameLoop deeperLoop = new PrototypeGameLoop();
        deeperLoop.Initialize(true, 1337);
        Require(deeperLoop.habitats.Count >= 3, "deeper cave scenario needs multiple habitat layers");
        Require(deeperLoop.EnterHabitat(0).success, "deeper cave scenario should enter the first habitat");
        Require(deeperLoop.PlaceTrap().success, "deeper cave scenario trap placement should resolve");

        GameActionResult deeper = deeperLoop.TravelDeeper();
        Require(deeper.success, "travel deeper should succeed with the default habitat catalog");
        Require(deeperLoop.dayCycle.currentHabitatIndex == 1, "travel deeper should advance to habitat index 1");
        Require(deeperLoop.dayCycle.currentHabitat != null && deeperLoop.dayCycle.currentHabitat.depth == 2, "travel deeper should move to depth 2");

        int goldBeforeSafeRoom = deeperLoop.inventory.gold;
        int trapsBeforeSafeRoom = deeperLoop.inventory.traps;
        int safeRoomGoldCost = deeperLoop.dayCycle.currentHabitat.safeRoomGoldCost;
        int safeRoomTrapCost = deeperLoop.dayCycle.currentHabitat.safeRoomTrapCost;
        Require(deeperLoop.BuildSafeRoom().success, "safe room should be buildable in the deeper habitat");
        Require(deeperLoop.inventory.gold == goldBeforeSafeRoom - safeRoomGoldCost, "safe room should spend configured gold cost");
        Require(deeperLoop.inventory.traps == trapsBeforeSafeRoom - safeRoomTrapCost, "safe room should spend configured trap cost");
        Require(deeperLoop.currentDayReport.safeRoomsBuilt == 1, "safe room build should increment the day report count");
        Require(deeperLoop.currentDayReport.safeRoomTrapsSpent == safeRoomTrapCost, "safe room trap cost should be written to the day report");
        Require(deeperLoop.currentDayReport.totalExpense == safeRoomGoldCost + safeRoomTrapCost, "safe room gold and trap costs should be written as report expense");
        deeperLoop.inventory.AddDragon(deeperLoop.dragonTypes[0].name, true);
        int expectedSafeRoomEndingScore = deeperLoop.scoring.aliveDragonPoints;
        int dayBeforeSafeNight = deeperLoop.dayCycle.day;
        GameActionResult safeNight = deeperLoop.EndDay();
        Require(safeNight.success && safeNight.dayAdvanced, "safe room night should advance the day");
        Require(deeperLoop.dayCycle.day == dayBeforeSafeNight + 1, "safe room night should advance exactly one day");
        Require(MessageContains(safeNight, "Guvenli odada"), "safe room night should explain safe room survival");
        Require(deeperLoop.dayCycle.IsInHabitat, "safe room night should keep the loop in habitat");
        DayReport safeRoomReport = deeperLoop.GetLastDayReport();
        Require(safeRoomReport != null && safeRoomReport.finalized, "safe room EndDay should finalize a day report");
        Require(!safeRoomReport.runResetAtDayEnd, "safe room EndDay should not mark run reset");
        Require(safeRoomReport.safeRoomsBuilt == 1 && safeRoomReport.safeRoomTrapsSpent == safeRoomTrapCost, "safe room report should preserve safe room trap cost and count");
        Require(safeRoomReport.totalExpense == safeRoomGoldCost + safeRoomTrapCost, "safe room report should preserve gold and trap expenses");
        Require(safeRoomReport.endingScore == expectedSafeRoomEndingScore, "safe room EndDay report should preserve ending score");

        PrototypeGameLoop noGoldSafeRoomLoop = new PrototypeGameLoop();
        noGoldSafeRoomLoop.Initialize(true, 1337);
        Require(noGoldSafeRoomLoop.EnterHabitat(0).success, "no-gold safe room test should enter habitat");
        noGoldSafeRoomLoop.inventory.gold = noGoldSafeRoomLoop.dayCycle.currentHabitat.safeRoomGoldCost - 1;
        GameActionResult noGoldSafeRoom = noGoldSafeRoomLoop.BuildSafeRoom();
        Require(!noGoldSafeRoom.success && MessageContains(noGoldSafeRoom, "altin"), "safe room should fail clearly when gold is insufficient");

        PrototypeGameLoop noTrapSafeRoomLoop = new PrototypeGameLoop();
        noTrapSafeRoomLoop.Initialize(true, 1337);
        Require(noTrapSafeRoomLoop.EnterHabitat(1).success, "no-trap safe room test should enter habitat with trap cost");
        noTrapSafeRoomLoop.inventory.traps = noTrapSafeRoomLoop.dayCycle.currentHabitat.safeRoomTrapCost - 1;
        GameActionResult noTrapSafeRoom = noTrapSafeRoomLoop.BuildSafeRoom();
        Require(!noTrapSafeRoom.success && MessageContains(noTrapSafeRoom, "tuzak"), "safe room should fail clearly when traps are insufficient");

        PrototypeGameLoop reportLoop = new PrototypeGameLoop();
        reportLoop.Initialize(true, 1337);
        int reportStartGold = reportLoop.inventory.gold;
        int trapBuyCost = reportLoop.economy.trapBuyPrice;
        Require(reportLoop.BuyTrap().success, "day report buy trap test should succeed");
        int rabbitBuyCost = reportLoop.economy.rabbitBuyPrice;
        Require(reportLoop.BuyRabbit().success, "day report buy rabbit test should succeed");
        Require(reportLoop.currentDayReport.startingGold == reportStartGold, "day report should store starting gold");
        Require(reportLoop.currentDayReport.trapsBought == 1, "day report should count bought traps");
        Require(reportLoop.currentDayReport.rabbitsBought == 1, "day report should count bought rabbits");
        Require(reportLoop.currentDayReport.totalExpense == trapBuyCost + rabbitBuyCost, "day report should include market purchases as expense");

        string reportDragonName = reportLoop.dragonTypes[0].name;
        int reportDragonPrice = reportLoop.dragonTypes[0].alivePrice;
        reportLoop.inventory.AddDragon(reportDragonName, true);
        Require(reportLoop.SellDragon(reportDragonName, true).success, "day report dragon sale test should succeed");
        Require(reportLoop.currentDayReport.liveDragonsSold == 1, "day report should count live dragon sales");
        Require(reportLoop.currentDayReport.totalIncome == reportDragonPrice, "day report should include dragon sales as income");
        Require(reportLoop.currentDayReport.netProfit == reportLoop.currentDayReport.totalIncome - reportLoop.currentDayReport.totalExpense, "day report net should equal income minus expense");
        Require(reportLoop.EndDay().success, "day report EndDay should succeed in market");
        DayReport completedReport = reportLoop.GetLastDayReport();
        Require(completedReport != null && completedReport.finalized, "EndDay should expose last completed day report");
        Require(completedReport.endingGold == reportLoop.inventory.gold, "completed day report should store ending gold");
        Require(completedReport.netProfit == completedReport.totalIncome - completedReport.totalExpense, "completed day report net should equal income minus expense");

        int expectedScore = deeperLoop.inventory.dragonsAlive * deeperLoop.scoring.aliveDragonPoints + deeperLoop.inventory.dragonsDead * deeperLoop.scoring.deadDragonPoints;
        Require(deeperLoop.scoring.currentScore == expectedScore, "score should only count held dragons");

        PrototypeGameLoop sellLoop = new PrototypeGameLoop();
        sellLoop.Initialize(true, 1337);
        string liveDragonName = sellLoop.dragonTypes[0].name;
        string deadDragonName = sellLoop.dragonTypes[1].name;
        sellLoop.inventory.AddDragon(liveDragonName, true);
        sellLoop.inventory.AddDragon(deadDragonName, false);
        int startingGold = sellLoop.inventory.gold;
        int livePrice = sellLoop.dragonTypes[0].alivePrice;
        int deadPrice = sellLoop.dragonTypes[1].deadPrice;

        GameActionResult sellAlive = sellLoop.SellDragon(liveDragonName, true);
        Require(sellAlive.success, "selected live dragon sale should succeed by name");
        Require(sellLoop.inventory.GetDragonCount(liveDragonName, true) == 0, "selected live dragon sale should remove one live dragon");
        Require(sellLoop.inventory.gold == startingGold + livePrice, "selected live dragon sale should add live sale gold");
        Require(sellAlive.score == sellLoop.scoring.deadDragonPoints, "selected live dragon sale should reduce score to remaining dead dragon");

        GameActionResult sellDead = sellLoop.SellDragon(deadDragonName, false);
        Require(sellDead.success, "selected dead dragon sale should succeed by name");
        Require(sellLoop.inventory.GetDragonCount(deadDragonName, false) == 0, "selected dead dragon sale should remove one dead dragon");
        Require(sellLoop.inventory.gold == startingGold + livePrice + deadPrice, "selected dead dragon sale should add dead sale gold");
        Require(sellDead.score == 0, "selling all dragons should reduce held score to zero");

        GameActionResult wrongNameSale = sellLoop.SellDragon("missing-dragon", true);
        Require(!wrongNameSale.success && !string.IsNullOrEmpty(wrongNameSale.message), "wrong dragon name should return a failure message");

        GameActionResult emptyStockSale = sellLoop.SellDragon(liveDragonName, true);
        Require(!emptyStockSale.success && !string.IsNullOrEmpty(emptyStockSale.message), "selling a dragon with no inventory count should return a failure message");

        PrototypeGameLoop noRabbitBaitLoop = new PrototypeGameLoop();
        noRabbitBaitLoop.Initialize(true, 1337);
        Require(noRabbitBaitLoop.EnterHabitat(0).success, "no-rabbit baited trap test should enter habitat");
        GameActionResult noRabbitBait = noRabbitBaitLoop.PlaceTrapWithRabbit();
        Require(!noRabbitBait.success && MessageContains(noRabbitBait, "tavsan"), "baited trap should fail with a message when no rabbit is available");

        PrototypeGameLoop baitedTrapLoop = new PrototypeGameLoop();
        baitedTrapLoop.Initialize(true, 1337);
        baitedTrapLoop.inventory.AddRabbits(1);
        Require(baitedTrapLoop.EnterHabitat(0).success, "baited trap test should enter habitat");
        int rabbitsBeforeBaitedTrap = baitedTrapLoop.inventory.rabbits;
        GameActionResult baitedTrap = baitedTrapLoop.PlaceTrapWithRabbit();
        Require(baitedTrap.success, "baited trap should resolve when a rabbit is available");
        Require(baitedTrapLoop.inventory.rabbits == rabbitsBeforeBaitedTrap - 1, "baited trap should consume exactly one rabbit");
        Require(baitedTrapLoop.trapSystem.lastTrapKind == TrapKind.RabbitBait, "baited trap should stamp rabbit bait trap kind");
        Require(baitedTrapLoop.currentDayReport.rabbitBaitTrapsPlaced == 1, "day report should count baited traps");
        Require(MessageContains(baitedTrap, "yemli"), "baited trap result message should mention bait");

        PrototypeGameLoop normalTrapLoop = new PrototypeGameLoop();
        normalTrapLoop.Initialize(true, 1337);
        normalTrapLoop.inventory.AddRabbits(1);
        normalTrapLoop.habitats.Get(0).rabbitChance = 0f;
        Require(normalTrapLoop.EnterHabitat(0).success, "normal trap rabbit consumption test should enter habitat");
        int rabbitsBeforeNormalTrap = normalTrapLoop.inventory.rabbits;
        GameActionResult normalTrap = normalTrapLoop.PlaceTrap();
        Require(normalTrap.success, "normal trap should resolve while carrying rabbits");
        Require(normalTrapLoop.inventory.rabbits == rabbitsBeforeNormalTrap, "normal trap should not consume a carried rabbit");
        Require(normalTrapLoop.trapSystem.lastTrapKind == TrapKind.Simple, "normal trap should stamp simple trap kind");
        Require(normalTrapLoop.currentDayReport.normalTrapsPlaced == 1, "day report should count normal traps");

        PrototypeGameLoop normalTrapTimeLoop = new PrototypeGameLoop();
        normalTrapTimeLoop.Initialize(true, 1337);
        normalTrapTimeLoop.habitats.Get(0).rabbitChance = 0f;
        normalTrapTimeLoop.habitats.Get(0).dragonEncounterChance = 0f;
        Require(normalTrapTimeLoop.EnterHabitat(0).success, "normal trap time test should enter habitat");
        float normalTrapTimeBefore = normalTrapTimeLoop.timeSystem.timeLeft;
        Require(normalTrapTimeLoop.PlaceTrap().success, "normal trap time test should resolve");
        float normalTrapSecondsSpent = normalTrapTimeBefore - normalTrapTimeLoop.timeSystem.timeLeft;

        PrototypeGameLoop baitedTrapTimeLoop = new PrototypeGameLoop();
        baitedTrapTimeLoop.Initialize(true, 1337);
        baitedTrapTimeLoop.inventory.AddRabbits(1);
        baitedTrapTimeLoop.habitats.Get(0).rabbitChance = 0f;
        baitedTrapTimeLoop.habitats.Get(0).dragonEncounterChance = 0f;
        Require(baitedTrapTimeLoop.EnterHabitat(0).success, "baited trap time test should enter habitat");
        float baitedTrapTimeBefore = baitedTrapTimeLoop.timeSystem.timeLeft;
        Require(baitedTrapTimeLoop.PlaceTrapWithRabbit().success, "baited trap time test should resolve");
        float baitedTrapSecondsSpent = baitedTrapTimeBefore - baitedTrapTimeLoop.timeSystem.timeLeft;
        Require(Mathf.Abs(normalTrapSecondsSpent - normalTrapTimeLoop.dayCycle.currentHabitat.trapPlacementSeconds) < 0.01f, "normal trap should spend base trap placement time");
        Require(Mathf.Abs(baitedTrapSecondsSpent - (baitedTrapTimeLoop.dayCycle.currentHabitat.trapPlacementSeconds + baitedTrapTimeLoop.trapSystem.rabbitBaitExtraSeconds)) < 0.01f, "baited trap should spend base time plus rabbit bait extra time");
        Require(baitedTrapSecondsSpent > normalTrapSecondsSpent, "baited trap should spend more time than normal trap");

        PrototypeGameLoop clearLoop = new PrototypeGameLoop();
        clearLoop.Initialize(true, 1337);
        clearLoop.dragonCaptureSystem.unbaitedCaptureChance = 1f;
        Require(clearLoop.EnterHabitat(0).success, "habitat clear test should enter habitat");
        clearLoop.dayCycle.currentHabitat.rabbitChance = 0f;
        clearLoop.dayCycle.currentHabitat.dragonEncounterChance = 1f;
        clearLoop.dayCycle.currentHabitat.captureModifier = 1f;
        clearLoop.dayCycle.currentHabitat.requiredCaptures = 1;
        clearLoop.dayCycle.currentHabitat.requiredScore = 1;
        GameActionResult clearResult = clearLoop.PlaceTrap();
        Require(clearResult.success && clearResult.habitatCleared, "habitat clear condition should trigger after required capture");
        Require(clearResult.runState == RunTargetState.HabitatCleared, "habitat clear should stamp habitat cleared state");
        Require(MessageContains(clearResult, "temizlendi"), "habitat clear should return a clear message");
        Require(clearLoop.currentDayReport.liveDragonsCaptured + clearLoop.currentDayReport.deadDragonsCaptured == 1, "day report should count captured dragons");
        Require(clearLoop.currentDayReport.habitatCleared, "day report should mark habitat cleared");

        PrototypeGameLoop allCavesLoop = new PrototypeGameLoop();
        allCavesLoop.Initialize(true, 1337);
        allCavesLoop.dragonCaptureSystem.unbaitedCaptureChance = 1f;
        allCavesLoop.inventory.AddTraps(10);
        for (int i = 0; i < allCavesLoop.habitats.Count; i++)
        {
            HabitatModel habitat = allCavesLoop.habitats.Get(i);
            habitat.enterSeconds = 0f;
            habitat.exitSeconds = 0f;
            habitat.trapPlacementSeconds = 0f;
            habitat.rabbitChance = 0f;
            habitat.dragonEncounterChance = 1f;
            habitat.captureModifier = 1f;
            habitat.requiredCaptures = 1;
            habitat.requiredScore = 1;

            Require(allCavesLoop.EnterHabitat(i).success, "all-caves test should enter each habitat");
            GameActionResult clearEach = allCavesLoop.PlaceTrap();
            if (i < allCavesLoop.habitats.Count - 1)
            {
                Require(clearEach.habitatCleared && clearEach.runState == RunTargetState.HabitatCleared, "each non-final habitat should clear");
                Require(allCavesLoop.GoToMarket().success, "all-caves test should return to market between habitats");
            }
            else
            {
                Require(clearEach.allCavesCompleted, "final habitat clear should complete all caves");
                Require(clearEach.runState == RunTargetState.AllCavesCompleted, "final habitat clear should stamp all caves completed state");
            }
        }

        Debug.Log(deeperLoop.BuildStatusText());
    }
    private static bool MessageContains(GameActionResult result, string value)
    {
        return result != null
            && !string.IsNullOrEmpty(result.message)
            && result.message.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.Exception("Core gameplay smoke test failed: " + message);
        }
    }

    private GameActionResult CompleteTrapAction(GameActionResult result, int aliveBefore, int deadBefore, bool useRabbitBait)
    {
        if (result.success && dayCycle.IsInHabitat)
        {
            EnsureCurrentDayReport();
            if (useRabbitBait)
            {
                currentDayReport.rabbitBaitTrapsPlaced++;
            }
            else
            {
                currentDayReport.normalTrapsPlaced++;
            }

            int aliveDelta = Mathf.Max(0, inventory.dragonsAlive - aliveBefore);
            int deadDelta = Mathf.Max(0, inventory.dragonsDead - deadBefore);
            int captureCount = aliveDelta + deadDelta;

            if (captureCount > 0)
            {
                int captureScore = aliveDelta * scoring.aliveDragonPoints + deadDelta * scoring.deadDragonPoints;
                dayCycle.RecordHabitatCapture(captureScore);
                currentDayReport.liveDragonsCaptured += aliveDelta;
                currentDayReport.deadDragonsCaptured += deadDelta;
            }
        }

        return Complete(result);
    }

    private void EvaluateHabitatCompletion(GameActionResult result)
    {
        if (!dayCycle.IsInHabitat || dayCycle.currentHabitat == null)
        {
            return;
        }

        if (!dayCycle.IsCurrentHabitatCleared())
        {
            return;
        }

        bool newlyCleared = dayCycle.MarkCurrentHabitatCleared();
        bool allCleared = dayCycle.AreAllHabitatsCleared(habitats.Count);

        result.habitatCleared = true;
        if (allCleared)
        {
            runState = RunTargetState.AllCavesCompleted;
            result.allCavesCompleted = true;
            if (newlyCleared)
            {
                result.AppendMessage(dayCycle.currentHabitat.displayName + " temizlendi. Tum magara tamamlandi.");
            }

            return;
        }

        runState = RunTargetState.HabitatCleared;
        if (newlyCleared)
        {
            result.AppendMessage(dayCycle.currentHabitat.displayName + " temizlendi. Derine inebilir veya pazara donebilirsin.");
        }
    }

    private void TrackReportOutcomeFlags(GameActionResult result)
    {
        if (result == null)
        {
            return;
        }

        EnsureCurrentDayReport();
        currentDayReport.habitatCleared = currentDayReport.habitatCleared || result.habitatCleared;
        currentDayReport.allCavesCompleted = currentDayReport.allCavesCompleted || result.allCavesCompleted;
    }

    private GameActionResult Complete(GameActionResult result)
    {
        if (result.success)
        {
            EvaluateHabitatCompletion(result);
            TrackReportOutcomeFlags(result);
        }

        if (!string.IsNullOrEmpty(result.message))
        {
            Log(result.message);
        }

        if (result.success && timeSystem.IsDayOver)
        {
            GameActionResult night = ResolveNight("Zaman bitti.");
            result.AppendMessage(night.message);
            result.dayAdvanced = night.dayAdvanced;
            result.runReset = night.runReset;
            Log(night.message);
        }

        Stamp(result);
        return result;
    }

    private GameActionResult ResolveNight(string cause)
    {
        if (dayCycle.IsInHabitat && !dayCycle.hasSafeRoom)
        {
            FinalizeCurrentDayReport(true, inventory.gold);
            ResetAfterDeath();
            StartDayReport();
            GameActionResult death = GameActionResult.Success(cause + " Guvenli oda yoktu; geceyi magarada geciremedin. Kosu basarisiz oldu ve pazar/gun 1 durumuna resetlendi.");
            death.runReset = true;
            return death;
        }

        GameLocation nextLocation = dayCycle.IsInHabitat ? GameLocation.Habitat : GameLocation.Market;
        HabitatModel habitat = dayCycle.currentHabitat;
        int habitatIndex = dayCycle.currentHabitatIndex;

        FinalizeCurrentDayReport(false, inventory.gold);
        dayCycle.StartNewDay(nextLocation, habitat, habitatIndex);
        timeSystem.ResetDay();
        economy.NewDay(dayCycle.day, random);
        StartDayReport();

        string placeMessage = nextLocation == GameLocation.Habitat
            ? "Guvenli odada geceyi atlattin; ayni habitatta yeni gune basladin."
            : "Pazarda geceyi atlattin.";
        GameActionResult result = GameActionResult.Success(string.Format("{0} {1} Gun {2}.", cause, placeMessage, dayCycle.day));
        result.dayAdvanced = true;
        return result;
    }

    private void StartDayReport()
    {
        currentDayReport = new DayReport();
        currentDayReport.Begin(dayCycle.day, inventory.gold);
    }

    private void EnsureCurrentDayReport()
    {
        if (currentDayReport == null)
        {
            StartDayReport();
        }
    }

    private void FinalizeCurrentDayReport(bool runReset, int endingGold)
    {
        EnsureCurrentDayReport();
        int endingScore = scoring.Recalculate(inventory, dayCycle);
        currentDayReport.Complete(
            endingGold,
            endingScore,
            runReset,
            currentDayReport.habitatCleared,
            currentDayReport.allCavesCompleted);
        lastCompletedDayReport = currentDayReport;
        currentDayReport = null;
    }

    private void ResetAfterDeath()
    {
        dayCycle.ResetRun();
        timeSystem.ResetDay();
        inventory.ResetForNewRun(dragonTypes);
        economy.ResetForNewRun(dayCycle.day);
        scoring.Recalculate(inventory, dayCycle);
        runState = RunTargetState.RunFailedReset;
    }

    private void EnsureInitialized()
    {
        if (!initialized)
        {
            Initialize(false, 0);
        }
    }

    private void EnsureContent()
    {
        if (dragonTypes == null || dragonTypes.Length == 0)
        {
            dragonTypes = DragonType.CreateDefaultRoster();
        }

        habitats.EnsureDefaults();
        inventory.EnsureDragonTypes(dragonTypes);
    }

    private DragonType FindDragonType(string dragonName)
    {
        if (string.IsNullOrEmpty(dragonName) || dragonTypes == null)
        {
            return null;
        }

        for (int i = 0; i < dragonTypes.Length; i++)
        {
            DragonType dragonType = dragonTypes[i];
            if (dragonType != null && dragonType.name == dragonName)
            {
                return dragonType;
            }
        }

        return null;
    }

    private void Stamp(GameActionResult result)
    {
        result.day = dayCycle.day;
        result.score = scoring.Recalculate(inventory, dayCycle);
        result.runState = runState;
        result.habitatCleared = result.habitatCleared || runState == RunTargetState.HabitatCleared || runState == RunTargetState.AllCavesCompleted;
        result.allCavesCompleted = result.allCavesCompleted || runState == RunTargetState.AllCavesCompleted;
    }

    private void Log(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        eventLog.Add(message);
        Debug.Log(message);
    }
}
