using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PrototypeGame : MonoBehaviour
{
    // =======================
    //   DATA STRUCTURES
    // =======================

    public enum GameState { Market, Cave, Night }
    public GameState state = GameState.Market;

    public enum SellItemKind { Trap, Rabbit, DragonAlive, DragonDead }

    [System.Serializable]
    public class SellOption
    {
        public SellItemKind kind;
        public string dragonName; // sadece dragonlarda dolu
    }

    [System.Serializable]
    public class DragonType
    {
        public string name;
        [Range(1, 5)] public int rarity;            // 1=common, 5=legendary
        [Range(-0.3f, 0.3f)] public float catchMod; // yakalama þansýna eklenir
        public int alivePrice;
        public int deadPrice;

        public DragonType(string name, int rarity, float catchMod, int alivePrice, int deadPrice)
        {
            this.name = name;
            this.rarity = rarity;
            this.catchMod = catchMod;
            this.alivePrice = alivePrice;
            this.deadPrice = deadPrice;
        }
    }

    // =======================
    //   CORE
    // =======================

    [Header("Core")]
    public int day = 1;
    public int gold = 100;

    [Header("Inventory")]
    public int traps = 2;
    public int rabbits = 0;

    [Header("Dragons (Totals)")]
    public int dragonsAlive = 0;
    public int dragonsDead = 0;

    // Tür bazlý sayým
    private Dictionary<string, int> aliveByType = new Dictionary<string, int>();
    private Dictionary<string, int> deadByType = new Dictionary<string, int>();

    [Header("Dragon Types")]
    public DragonType[] dragonTypes =
    {
        new DragonType("Kömürkanat", 1,  0.10f, 40, 10),
        new DragonType("Bakýrdiþ",  2,  0.05f, 60, 15),
        new DragonType("Buzsýrt",   3,  0.00f, 90, 25),
        new DragonType("Gölgepul",  4, -0.10f, 140, 40),
        new DragonType("Altýnkral", 5, -0.20f, 220, 70),
    };

    // =======================
    //   TIME
    // =======================

    [Header("Time")]
    public float dayDuration = 240f;
    private float timeLeft = 0f;
    private bool hasSafeRoom = false;
    public int caveDepth = 1;

    // =======================
    //   VENDOR (STOCK + DYNAMIC PRICE)
    // =======================

    [Header("Vendor Stock")]
    public int vendorTraps = 5;
    public int vendorRabbits = 3;

    [Header("Vendor Targets")]
    public int vendorTrapTarget = 6;
    public int vendorRabbitTarget = 4;

    [Header("Vendor Base Prices")]
    public int trapBaseBuy = 20;
    public int rabbitBaseBuy = 10;

    [Header("Vendor Prices (dynamic)")]
    public int trapBuyPrice;
    public int trapSellPrice;
    public int rabbitBuyPrice;
    public int rabbitSellPrice;

    // =======================
    //   UI
    // =======================

    [Header("UI")]
    public Text infoText;
    public Slider timeSlider;

    [Header("Market Buttons")]
    public Text buyTrapText;
    public Text buyRabbitText;
    public Text sellSelectedText;


    // Tek satýþ dropdown’u: Tuzak/Tavþan/Ejderha (canlý/ölü) hepsi burada
    public Dropdown sellDropdown;

    private List<SellOption> sellOptions = new List<SellOption>();

    public int score = 0;

    private string lastMessage = "";
    private string lastMessageKey = "";
    private int lastMessageRepeat = 0;

    // =======================
    //   UNITY LIFECYCLE
    // =======================

    void Start()
    {
        timeLeft = dayDuration;

        // Tür sözlüklerini hazýrla
        foreach (var dt in dragonTypes)
        {
            if (!aliveByType.ContainsKey(dt.name)) aliveByType.Add(dt.name, 0);
            if (!deadByType.ContainsKey(dt.name)) deadByType.Add(dt.name, 0);
        }

        // Ýlk gün satýcý + fiyat
        VendorNewDay(silentLog: true);

        RefreshSellDropdown();
        UpdateInfo();
        UpdateMarketButtonTexts();

    }

    // =======================
    //   SELL DROPDOWN
    // =======================

    private void RefreshSellDropdown()
    {
        if (sellDropdown == null) return;

        sellDropdown.ClearOptions();
        sellOptions.Clear();

        List<string> labels = new List<string>();

        // Tuzak
        if (traps > 0)
        {
            sellOptions.Add(new SellOption { kind = SellItemKind.Trap });
            labels.Add($"Tuzak ({traps}) | Sat:{trapSellPrice}");
        }

        // Tavþan
        if (rabbits > 0)
        {
            sellOptions.Add(new SellOption { kind = SellItemKind.Rabbit });
            labels.Add($"Tavþan ({rabbits}) | Sat:{rabbitSellPrice}");
        }

        // Ejderhalar (sadece elindekiler)
        foreach (var dt in dragonTypes)
        {
            int a = aliveByType.ContainsKey(dt.name) ? aliveByType[dt.name] : 0;
            int d = deadByType.ContainsKey(dt.name) ? deadByType[dt.name] : 0;

            if (a > 0)
            {
                sellOptions.Add(new SellOption { kind = SellItemKind.DragonAlive, dragonName = dt.name });
                labels.Add($"Canlý {dt.name} ({a}) | Sat:{dt.alivePrice}");
            }

            if (d > 0)
            {
                sellOptions.Add(new SellOption { kind = SellItemKind.DragonDead, dragonName = dt.name });
                labels.Add($"Ölü {dt.name} ({d}) | Sat:{dt.deadPrice}");
            }
        }

        if (labels.Count == 0)
        {
            labels.Add("Satýlacak bir þey yok");
        }

        sellDropdown.AddOptions(labels);
        UpdateMarketButtonTexts();

    }
    private void UpdateMarketButtonTexts()
    {
        // Al butonlarý
        if (buyTrapText != null)
            buyTrapText.text = $"Tuzak Al ({trapBuyPrice})";

        if (buyRabbitText != null)
            buyRabbitText.text = $"Tavþan Al ({rabbitBuyPrice})";

        // Sat butonu
        if (sellSelectedText == null || sellOptions.Count == 0)
        {
            if (sellSelectedText != null)
                sellSelectedText.text = "Sat";
            return;
        }

        int index = sellDropdown.value;
        if (index < 0 || index >= sellOptions.Count)
        {
            sellSelectedText.text = "Sat";
            return;
        }

        var opt = sellOptions[index];
        int price = 0;

        switch (opt.kind)
        {
            case SellItemKind.Trap:
                price = trapSellPrice;
                break;

            case SellItemKind.Rabbit:
                price = rabbitSellPrice;
                break;

            case SellItemKind.DragonAlive:
                {
                    var dt = System.Array.Find(dragonTypes, d => d.name == opt.dragonName);
                    if (dt != null) price = dt.alivePrice;
                    break;
                }

            case SellItemKind.DragonDead:
                {
                    var dt = System.Array.Find(dragonTypes, d => d.name == opt.dragonName);
                    if (dt != null) price = dt.deadPrice;
                    break;
                }
        }

        sellSelectedText.text = $"Sat (+{price})";
    }
    public void OnSellDropdownChanged()
    {
        UpdateMarketButtonTexts();
    }


    public void SellSelectedItem1()
    {
        if (!SpendTime(4f, "Pazar iþlemi")) return;
        if (state != GameState.Market) { Log("Satýþ sadece pazarda yapýlýr."); return; }

        if (sellOptions.Count == 0)
        {
            Log("Satýlacak bir þey yok!");
            return;
        }

        int index = sellDropdown.value;
        if (index < 0 || index >= sellOptions.Count) return;

        var opt = sellOptions[index];

        switch (opt.kind)
        {
            case SellItemKind.Trap:
                if (traps <= 0) { Log("Satacak tuzaðýn yok!"); return; }
                traps--;
                gold += trapSellPrice;
                vendorTraps++; // satýcý stoðuna geri girsin (MVP)
                Log($"1 tuzak sattýn. (+{trapSellPrice} altýn)");
                break;

            case SellItemKind.Rabbit:
                if (rabbits <= 0) { Log("Satacak tavþanýn yok!"); return; }
                rabbits--;
                gold += rabbitSellPrice;
                vendorRabbits++;
                Log($"1 tavþan sattýn. (+{rabbitSellPrice} altýn)");
                break;

            case SellItemKind.DragonAlive:
                {
                    string name = opt.dragonName;
                    if (!aliveByType.ContainsKey(name) || aliveByType[name] <= 0) { Log("Canlý ejderha kalmadý."); return; }

                    var dt = System.Array.Find(dragonTypes, d => d.name == name);
                    if (dt == null) return;

                    aliveByType[name]--;
                    dragonsAlive--;
                    gold += dt.alivePrice;
                    Log($"1 canlý {name} sattýn. (+{dt.alivePrice} altýn)");
                    break;
                }

            case SellItemKind.DragonDead:
                {
                    string name = opt.dragonName;
                    if (!deadByType.ContainsKey(name) || deadByType[name] <= 0) { Log("Ölü ejderha kalmadý."); return; }

                    var dt = System.Array.Find(dragonTypes, d => d.name == name);
                    if (dt == null) return;

                    deadByType[name]--;
                    dragonsDead--;
                    gold += dt.deadPrice;
                    Log($"1 ölü {name} sattýn. (+{dt.deadPrice} altýn)");
                    break;
                }
        }

        // Satýþ sonrasý: fiyatlar/ekran güncellensin
        RecalculateVendorPrices();
        RefreshSellDropdown();
        UpdateInfo();
    }

    // =======================
    //   VENDOR BUY/SELL (TRAP/RABBIT)
    // =======================

    public void VendorBuyTrap1()
    {
        if (!SpendTime(4f, "Pazar iþlemi")) return;
        if (state != GameState.Market) { Log("Bu iþlem sadece pazarda yapýlýr."); return; }

        if (vendorTraps <= 0) { Log("Satýcýda tuzak kalmadý!"); return; }
        if (gold < trapBuyPrice) { Log($"Altýn yetmiyor! (Tuzak {trapBuyPrice})"); return; }

        gold -= trapBuyPrice;
        traps += 1;
        vendorTraps -= 1;

        RecalculateVendorPrices();
        RefreshSellDropdown();
        Log($"Satýcýdan 1 tuzak aldýn. (-{trapBuyPrice} altýn)");
    }

    public void VendorBuyRabbit1()
    {
        if (!SpendTime(4f, "Pazar iþlemi")) return;
        if (state != GameState.Market) { Log("Bu iþlem sadece pazarda yapýlýr."); return; }

        if (vendorRabbits <= 0) { Log("Satýcýda tavþan kalmadý!"); return; }
        if (gold < rabbitBuyPrice) { Log($"Altýn yetmiyor! (Tavþan {rabbitBuyPrice})"); return; }

        gold -= rabbitBuyPrice;
        rabbits += 1;
        vendorRabbits -= 1;

        RecalculateVendorPrices();
        RefreshSellDropdown();
        Log($"Satýcýdan 1 tavþan aldýn. (-{rabbitBuyPrice} altýn)");
    }

    // (Ýstersen butonla satýcýya geri satmayý ayrý tutabilirsin; biz artýk unified satýþtan satýyoruz.)
    // VendorSellTrap1 / VendorSellRabbit1 istersen kalabilir ama þart deðil.

    // =======================
    //   GAME FLOW
    // =======================

    private void StartNewDay(string msg)
    {
        day++;
        timeLeft = dayDuration;
        hasSafeRoom = false;

        VendorNewDay(silentLog: false);

        Log($"{msg} Gün: {day}");
        RefreshSellDropdown();
        UpdateInfo();
    }

    public void GoHome()
    {
        if (!SpendTime(20f, "Eve dönüþ")) return;

        state = GameState.Market;
        StartNewDay("Eve giderek günü bitirdin.");
    }

    public void GoMarket()
    {
        if (!SpendTime(25f, "Pazara yolculuk")) return;

        state = GameState.Market;
        hasSafeRoom = false;

        Log("Pazardasýn. Alýþveriþ yap.");
        RefreshSellDropdown();
        UpdateInfo();
    }

    public void EnterCave()
    {
        if (state == GameState.Cave) { Log("Zaten maðaradasýn!"); return; }
        if (!SpendTime(25f, "Maðaraya yolculuk")) return;

        state = GameState.Cave;
        caveDepth = 1;
        Log("Maðaraya girdin.");
        UpdateInfo();
    }

    public void GoDeeper()
    {
        if (state != GameState.Cave)
        {
            Log("Daha derine sadece maðarada inebilirsin.");
            return;
        }

        if (!SpendTime(20f, "Maðarada daha derine inme")) return;

        caveDepth++;
        Log($"Maðarada daha derine indin. Maðara Katý: {caveDepth}");
        UpdateInfo();
    }

    public void ExitCave()
    {
        if (state != GameState.Cave) { Log("Þu an maðarada deðilsin."); return; }
        if (!SpendTime(15f, "Maðaradan çýkýþ")) return;

        state = GameState.Market;
        hasSafeRoom = false;

        Log("Maðaradan çýktýn, pazara döndün.");
        RefreshSellDropdown();
        UpdateInfo();
    }

    public void BuildSafeRoom()
    {
        if (state != GameState.Cave) { Log("Güvenli oda sadece maðarada kurulur."); return; }
        if (hasSafeRoom) { Log("Zaten güvenli oda kurdun."); return; }
        if (!SpendTime(40f, "Güvenli oda inþasý")) return;

        hasSafeRoom = true;
        Log("Güvenli oda kuruldu. Gece hayatta kalýrsýn.");
        UpdateInfo();
    }

    public void PlaceTrap()
    {
        if (state != GameState.Cave) { Log("Tuzak sadece maðarada kurulur."); return; }
        if (traps <= 0) { Log("Tuzak yok!"); return; }
        if (!SpendTime(12f, "Tuzak kurma")) return;

        traps--;

        float roll = Random.value;

        if (roll < 0.35f)
        {
            rabbits++;
            Log("Tuzak: Tavþan yakalandý!");
            RefreshSellDropdown();
            UpdateInfo();
            return;
        }

        DragonType dt = RollDragonType();

        float catchChance = (rabbits > 0) ? 0.65f : 0.05f;
        catchChance = Mathf.Clamp01(catchChance + dt.catchMod);

        if (Random.value < catchChance)
        {
            if (rabbits > 0) rabbits--;

            bool capturedAlive = Random.value < 0.75f;
            if (capturedAlive)
            {
                dragonsAlive++;
                aliveByType[dt.name]++;
                Log($"Tuzak: Canlý {dt.name} yakalandý! (R{dt.rarity})");
            }
            else
            {
                dragonsDead++;
                deadByType[dt.name]++;
                Log($"Tuzak: {dt.name} yakalandý ama öldü! (R{dt.rarity})");
            }

            RefreshSellDropdown();
            UpdateInfo();
        }
        else
        {
            Log("Tuzak boþa çýktý, ejderha kaçtý.");
            UpdateInfo();
        }
    }

    // =======================
    //   NIGHT
    // =======================

    private void GoNight()
    {
        GameState previousState = state;
        state = GameState.Night;

        if (previousState == GameState.Market)
        {
            StartNewDay("Pazarda geceyi geçirdin.");
            state = GameState.Market;
            return;
        }

        if (previousState == GameState.Cave)
        {
            if (!hasSafeRoom)
            {
                Log("Gece oldu ve güvenli odan yok... öldün. Reset.");
                ResetRun();
                return;
            }

            StartNewDay("Güvenli odada geceyi geçirdin.");
            state = GameState.Cave;
        }
    }

    private void ResetRun()
    {
        day = 1;
        gold = 100;
        traps = 2;
        rabbits = 0;
        caveDepth = 1;

        dragonsAlive = 0;
        dragonsDead = 0;

        foreach (var dt in dragonTypes)
        {
            aliveByType[dt.name] = 0;
            deadByType[dt.name] = 0;
        }

        state = GameState.Market;
        timeLeft = dayDuration;
        hasSafeRoom = false;

        VendorNewDay(silentLog: true);
        RefreshSellDropdown();
        UpdateInfo();
    }

    // =======================
    //   VENDOR LOGIC
    // =======================

    private void VendorNewDay(bool silentLog)
    {
        vendorTraps = Mathf.Clamp(vendorTraps + Random.Range(1, 4), 0, vendorTrapTarget);
        vendorRabbits = Mathf.Clamp(vendorRabbits + Random.Range(1, 3), 0, vendorRabbitTarget);

        RecalculateVendorPrices();

        if (!silentLog)
            Log("Satýcý stoklarýný yeniledi, fiyatlar güncellendi.");
    }

    private void RecalculateVendorPrices()
    {
        trapBuyPrice = CalcBuyPrice(trapBaseBuy, vendorTraps, vendorTrapTarget);
        rabbitBuyPrice = CalcBuyPrice(rabbitBaseBuy, vendorRabbits, vendorRabbitTarget);

        trapSellPrice = Mathf.Max(1, Mathf.RoundToInt(trapBuyPrice * 0.6f));
        rabbitSellPrice = Mathf.Max(1, Mathf.RoundToInt(rabbitBuyPrice * 0.6f));
        UpdateMarketButtonTexts();

    }

    private int CalcBuyPrice(int basePrice, int stock, int target)
    {
        float ratio = (target <= 0) ? 1f : Mathf.Clamp01(stock / (float)target);
        float multiplier = Mathf.Lerp(1.6f, 0.8f, ratio);
        float inflation = 1f + (day - 1) * 0.01f; // %1/gün
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * multiplier * inflation));
    }

    // =======================
    //   UTILS
    // =======================

    private bool SpendTime(float seconds, string reason)
    {
        if (timeLeft < seconds)
        {
            Log($"{reason} için süre yetmiyor! Gerekli: {seconds:0}s | Kalan: {timeLeft:0}s");
            return false;
        }

        timeLeft -= seconds;
        Log($"{reason} (-{seconds:0}s) | Kalan: {timeLeft:0}s");

        if (timeLeft <= 0f)
        {
            GoNight();
            return false;
        }

        return true;
    }

    private DragonType RollDragonType()
    {
        int minRarity = 1;
        int maxRarity = 5;

        if (caveDepth <= 1)
        {
            minRarity = 1;
            maxRarity = 2;
        }
        else if (caveDepth == 2)
        {
            minRarity = 1;
            maxRarity = 3;
        }
        else if (caveDepth == 3)
        {
            minRarity = 2;
            maxRarity = 4;
        }
        else
        {
            minRarity = 3;
            maxRarity = 5;
        }

        List<DragonType> candidates = new List<DragonType>();
        foreach (var dt in dragonTypes)
        {
            if (dt.rarity >= minRarity && dt.rarity <= maxRarity)
            {
                candidates.Add(dt);
            }
        }

        // Güvenlik: e?er filtrelenmi? liste bo?sa, eski davran??a dön.
        if (candidates.Count == 0)
        {
            float fallbackTotal = 0f;
            foreach (var dt in dragonTypes)
                fallbackTotal += 1f / (dt.rarity * dt.rarity);

            float fallbackR = Random.value * fallbackTotal;
            foreach (var dt in dragonTypes)
            {
                fallbackR -= 1f / (dt.rarity * dt.rarity);
                if (fallbackR <= 0f) return dt;
            }
            return dragonTypes[0];
        }

        float total = 0f;
        foreach (var dt in candidates)
            total += 1f / (dt.rarity * dt.rarity);

        float r = Random.value * total;
        foreach (var dt in candidates)
        {
            r -= 1f / (dt.rarity * dt.rarity);
            if (r <= 0f) return dt;
        }

        return candidates[0];
    }

    private void RecalculateScore()
    {
        score = dragonsAlive * 2 + dragonsDead;
    }

    private void UpdateInfo()
    {
        RecalculateScore();

        string header = lastMessage;
        if (lastMessageRepeat > 1) header += $" (x{lastMessageRepeat})";
        header += "\n\n";

        string text =
            header +
            $"Gün: {day}\n" +
            $"Durum: {state}\n" +
            $"Maðara Katý: {caveDepth}\n" +
            $"Altýn: {gold}\n" +
            $"Tuzak: {traps}\n" +
            $"Tavþan: {rabbits}\n" +
            $"Skor: {score}\n" +
            $"Zaman: {timeLeft:0.0}s\n" +
            $"\n--- Satýcý ---\n" +
            $"Tuzak Stok: {vendorTraps} (Al:{trapBuyPrice} / Sat:{trapSellPrice})\n" +
            $"Tavþan Stok: {vendorRabbits} (Al:{rabbitBuyPrice} / Sat:{rabbitSellPrice})\n";

        text += "\n--- Ejderha Türleri ---\n";
        foreach (var dt in dragonTypes)
        {
            int a = aliveByType[dt.name];
            int d = deadByType[dt.name];
            if (a > 0 || d > 0)
                text += $"{dt.name}  Canlý:{a} / Ölü:{d} (CanlýSat:{dt.alivePrice} ÖlüSat:{dt.deadPrice})\n";
        }

        if (infoText != null) infoText.text = text;

        if (timeSlider != null)
        {
            timeSlider.maxValue = dayDuration;
            timeSlider.value = timeLeft;
        }
        UpdateMarketButtonTexts();

    }
    public void Wait()
    {
        if (!SpendTime(5f, "Beklemek")) return;
        Log("Biraz bekledin.");
        UpdateInfo();
    }
    public void EndDay()
    {
        Log("Günü bitirmeyi seçtin.");
        GoNight();
    }

    private void Log(string msg)
    {
        Debug.Log(msg);

        if (msg == lastMessageKey)
        {
            lastMessageRepeat++;
        }
        else
        {
            lastMessageKey = msg;
            lastMessageRepeat = 1;
            lastMessage = msg;
        }

        UpdateInfo();
    }
}
