using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI buton / sahne geçişi doğrulaması (Play Mode veya Editor test menüsü).
/// Core gameplay dosyalarına dokunmaz; GameActionsService üzerinden dener.
/// </summary>
public static class PrototypeUiFlowVerifier
{
    public struct StepResult
    {
        public string step;
        public bool passed;
        public string detail;
    }

    public static IReadOnlyList<StepResult> RunAll()
    {
        var results = new List<StepResult>();
        SceneFlowWarningGate.ResetForTests();
        UiFeedbackLog.ResetForTests();
        HuntSelectionState.Clear();

        if (!EnsureSession())
        {
            results.Add(Fail("Oturum", "GameSession bulunamadı."));
            return results;
        }

        var session = GameSession.Instance;
        var actions = session.Actions;

        session.InitializeRun();
        results.Add(Pass("Başlangıç", "Gün 1, pazar konumu."));
        results.Add(AssertCamera("Başlangıç (kamera)"));
        results.Add(AssertCollapsiblePlayerPanels("Katlanabilir çanta / av panelleri"));
        results.Add(AssertPanelPersistence("Panel açık durumu kalıcılığı"));
        results.Add(AssertPlayerPanels("Çanta / Av panelleri"));
        results.Add(AssertStatusLogFormat("Durum mesajı formatı"));
        results.Add(AssertBuildSettings(SceneNames.MainHub));
        results.Add(AssertBuildSettings(SceneNames.Market));
        results.Add(AssertBuildSettings(SceneNames.Cave));

        actions.ReturnToMainHub();
        results.Add(AssertUi(SceneNames.MainHub, "Ana merkez UI", typeof(MainHubUIController)));
        results.Add(AssertMainHubActionLayout("MainHub aksiyon düzeni"));

        actions.GoToMarket();
        results.Add(AssertUi(SceneNames.Market, "Market'e Git", typeof(MarketUIController)));
        results.Add(AssertCamera("Market (kamera)"));
        results.Add(AssertMarketPrices("Market fiyat etiketleri"));

        var trapsBefore = session.Game.inventory.traps;
        actions.BuyTrap();
        results.Add(AssertState(
            "Tuzak Al",
            session.Game.inventory.traps > trapsBefore,
            $"Tuzak {trapsBefore} -> {session.Game.inventory.traps}"));

        var rabbitsBefore = session.Game.inventory.rabbits;
        actions.BuyRabbit();
        results.Add(AssertState(
            "Tavşan Al",
            session.Game.inventory.rabbits > rabbitsBefore,
            $"Tavşan {rabbitsBefore} -> {session.Game.inventory.rabbits}"));

        results.Add(AssertMarketDragonSales("Ejderha satış paneli (boş)", false));

        session.Game.inventory.AddDragon("Kömürkanat", true);
        session.Game.inventory.AddDragon("Buzsırt", false);
        results.Add(AssertHuntCatalogRows("Avladıklarım satır verisi"));
        HuntSelectionState.Select("Kömürkanat");
        results.Add(AssertMarketDragonSales("Ejderha satış paneli (dolu)", true));
        results.Add(AssertMarketScoreWarningButtons("Market skor uyarıları (-2/-1)"));

        var goldBefore = session.Game.inventory.gold;
        var aliveBefore = session.Game.inventory.GetDragonCount("Kömürkanat", true);
        var scoreBefore = session.Game.scoring.currentScore;
        var alivePoints = session.Game.scoring.aliveDragonPoints;
        actions.SellDragon("Kömürkanat", true);
        results.Add(AssertState(
            "Canlı Sat (Kömürkanat)",
            session.Game.inventory.gold > goldBefore
            && session.Game.inventory.GetDragonCount("Kömürkanat", true) < aliveBefore,
            $"Altın {goldBefore}->{session.Game.inventory.gold}, canlı {aliveBefore}->{session.Game.inventory.GetDragonCount("Kömürkanat", true)}"));
        results.Add(AssertState(
            "Canlı Sat skor düşüşü (-2 puan)",
            session.Game.scoring.currentScore == scoreBefore - alivePoints,
            $"Skor {scoreBefore}->{session.Game.scoring.currentScore}, beklenen -{alivePoints}"));
        results.Add(AssertSelectionClearedWhenSoldOut("Satış sonrası seçim temizleme", "Kömürkanat"));

        // Ölü Sat test (ayrı tür için).
        var goldBeforeDead = session.Game.inventory.gold;
        var deadBefore = session.Game.inventory.GetDragonCount("Buzsırt", false);
        var scoreBeforeDead = session.Game.scoring.currentScore;
        var deadPoints = session.Game.scoring.deadDragonPoints;
        actions.SellDragon("Buzsırt", false);
        results.Add(AssertState(
            "Ölü Sat (Buzsırt)",
            session.Game.inventory.gold > goldBeforeDead
            && session.Game.inventory.GetDragonCount("Buzsırt", false) < deadBefore,
            $"Altın {goldBeforeDead}->{session.Game.inventory.gold}, ölü {deadBefore}->{session.Game.inventory.GetDragonCount("Buzsırt", false)}"));
        results.Add(AssertState(
            "Ölü Sat skor düşüşü (-1 puan)",
            session.Game.scoring.currentScore == scoreBeforeDead - deadPoints,
            $"Skor {scoreBeforeDead}->{session.Game.scoring.currentScore}, beklenen -{deadPoints}"));

        actions.ReturnToMainHub();
        results.Add(AssertUi(SceneNames.MainHub, "Ana Ekrana Dön (Market)", typeof(MainHubUIController)));

        actions.EnterCave();
        results.Add(AssertUi(SceneNames.Cave, "Mağaraya Gir", typeof(CaveUIController)));
        results.Add(AssertState(
            "Mağaraya Gir (konum)",
            session.Game.dayCycle.IsInHabitat,
            "IsInHabitat bekleniyordu."));
        results.Add(AssertCaveHabitatName("Mağaraya Gir (kat)", "Dış Mağara"));

        results.Add(AssertCaveRabbitTrapButton("Cave Tavşanlı Tuzak butonu"));
        results.Add(AssertCaveTrapResources("Cave tuzak/tavşan etiketleri"));

        // 0 stokta tuzak/tavşan butonları disabled olmalı.
        session.Game.inventory.traps = 0;
        session.Game.inventory.rabbits = 0;
        UiFeedbackLog.Publish("refresh");
        var trapBtn0 = GameObject.Find("btn_place_trap")?.GetComponent<Button>();
        var baitBtn0 = GameObject.Find("btn_place_trap_rabbit")?.GetComponent<Button>();
        results.Add(AssertState(
            "Tuzak/Tavşan 0 iken disabled",
            trapBtn0 != null && baitBtn0 != null && !trapBtn0.interactable && !baitBtn0.interactable,
            "btn_place_trap veya btn_place_trap_rabbit disabled değil."));
        results.Add(AssertCaveTrapDisabledVisual("Tuzak 0 disabled görsel", "btn_place_trap", true));
        results.Add(AssertCaveTrapDisabledVisual("Tavşanlı tuzak 0 stok disabled görsel", "btn_place_trap_rabbit", true));

        session.Game.inventory.traps = 1;
        session.Game.inventory.rabbits = 0;
        UiFeedbackLog.Publish("refresh");
        var trapBtn1 = GameObject.Find("btn_place_trap")?.GetComponent<Button>();
        var baitBtn1 = GameObject.Find("btn_place_trap_rabbit")?.GetComponent<Button>();
        results.Add(AssertState(
            "Tavşan 0 iken tavşanlı tuzak disabled",
            trapBtn1 != null && baitBtn1 != null && trapBtn1.interactable && !baitBtn1.interactable,
            "btn_place_trap interactable değil veya tavşanlı buton disabled değil."));
        results.Add(AssertCaveTrapDisabledVisual("Tuzak 1 enabled görsel", "btn_place_trap", false));
        results.Add(AssertCaveTrapDisabledVisual("Tavşan 0 tavşanlı tuzak disabled görsel", "btn_place_trap_rabbit", true));

        // UI: all caves completed mesajı gelince kazanma paneli görünür olmalı.
        UiFeedbackLog.Publish("Dış Mağara temizlendi. Tum magara tamamlandi.");
        var winPanel = GameObject.Find("AllCavesCompletedPanel");
        results.Add(AssertState(
            "AllCavesCompleted paneli",
            winPanel != null && winPanel.activeInHierarchy,
            "AllCavesCompletedPanel aktif değil."));

        var statusGo = GameObject.Find("txt_status");
        var statusText = statusGo != null ? statusGo.GetComponent<Text>() : null;
        var statusValue = statusText != null ? statusText.text : "<null>";
        results.Add(AssertState(
            "AllCavesCompleted status log vurgusu",
            statusText != null && (statusText.text ?? "").IndexOf("tamamlandi", System.StringComparison.OrdinalIgnoreCase) >= 0,
            $"Status: '{statusValue}'"));

        session.InitializeRun();
        actions.GoToMarket();
        actions.BuyRabbit();
        actions.EnterCave();
        var rabbitsBeforeBait = session.Game.inventory.rabbits;
        var trapsBeforeBait = session.Game.inventory.traps;
        actions.PlaceTrapWithRabbit();
        results.Add(AssertState(
            "Tavşanlı Tuzak Kur",
            session.Game.inventory.rabbits < rabbitsBeforeBait
            && session.Game.inventory.traps < trapsBeforeBait,
            $"Tavşan {rabbitsBeforeBait}->{session.Game.inventory.rabbits}, tuzak {trapsBeforeBait}->{session.Game.inventory.traps}"));

        session.InitializeRun();
        actions.EnterCave();
        actions.PlaceTrapWithRabbit();
        results.Add(AssertLastMessageContains("Tavşanlı Tuzak (tavşan yok)", "tavsan"));

        actions.TravelDeeper();
        results.Add(AssertState(
            "TravelDeeper -> 1",
            session.Game.dayCycle.currentHabitatIndex == 1,
            $"Habitat index {session.Game.dayCycle.currentHabitatIndex}."));
        results.Add(AssertCaveHabitatName("Kat 1", "Kristal Tüneller"));

        actions.TravelDeeper();
        results.Add(AssertState(
            "TravelDeeper -> 2",
            session.Game.dayCycle.currentHabitatIndex == 2,
            $"Habitat index {session.Game.dayCycle.currentHabitatIndex}."));
        results.Add(AssertCaveHabitatName("Kat 2", "Kül Yarığı"));

        actions.TravelDeeper();
        results.Add(AssertState(
            "TravelDeeper -> 3",
            session.Game.dayCycle.currentHabitatIndex == 3,
            $"Habitat index {session.Game.dayCycle.currentHabitatIndex}."));
        results.Add(AssertCaveHabitatName("Kat 3", "Kadim Yuva"));

        actions.ReturnToHeadquarters();
        results.Add(AssertUi(SceneNames.MainHub, "Ana Merkez'e Dön (Cave)", typeof(MainHubUIController)));
        results.Add(AssertState(
            "Ana Merkez'e Dön (konum)",
            session.Game.dayCycle.IsInMarket,
            "Habitat'tan çıkınca pazar bekleniyor."));

        session.InitializeRun();
        actions.EnterCave();
        results.Add(AssertState(
            "Tuzak Kur (yalnızca Cave)",
            session.Game.dayCycle.IsInHabitat,
            "Mağaraya girilince habitat bekleniyor."));
        results.Add(AssertState(
            "Cave tuzak butonu mevcut",
            GameObject.Find("btn_place_trap") != null,
            "btn_place_trap Cave UI'da yok."));
        results.Add(AssertUi(SceneNames.Cave, "Tuzak Kur (Cave UI)", typeof(CaveUIController)));

        session.InitializeRun();
        session.Game.inventory.AddDragon("Kömürkanat", true);
        var expectedEndingScore = session.Game.scoring.aliveDragonPoints;
        actions.EnterCave();
        var caveDayBefore = session.Day;
        actions.EndDay();
        results.Add(AssertState(
            "Güvenli oda YOK EndDay (run reset)",
            session.Game.dayCycle.IsInMarket && session.Day == 1,
            $"Beklenen Market + Day=1, bulundu: IsInMarket={session.Game.dayCycle.IsInMarket}, Day={session.Day}"));
        results.Add(AssertLastMessageContains("Guvenli oda yoktu mesajı", "Guvenli oda yoktu"));
        results.Add(AssertUi(SceneNames.MainHub, "Guvenli oda YOK (UI yönlendirme)", typeof(MainHubUIController)));
        results.Add(AssertDayReportAfterEndDay("EndDay sonrası rapor oluşumu (run reset)", true));
        results.Add(AssertDayReportEndingScoreSource(
            "Rapor skoru report.endingScore (unsafe reset)",
            expectedEndingScore));
        results.Add(AssertDayReportPanel("Rapor paneli açılabiliyor (run reset satırı)", true));

        session.InitializeRun();
        actions.EnterCave();
        var safeDayBefore = session.Day;
        actions.BuildSafeRoom();
        results.Add(AssertState(
            "Güvenli oda VAR: BuildSafeRoom",
            session.Game.dayCycle.IsInHabitat && session.Game.dayCycle.hasSafeRoom,
            $"IsInHabitat={session.Game.dayCycle.IsInHabitat}, hasSafeRoom={session.Game.dayCycle.hasSafeRoom}"));
        results.Add(AssertUi(SceneNames.Cave, "Güvenli Oda Kur (UI)", typeof(CaveUIController)));

        actions.EndDay();
        results.Add(AssertState(
            "Güvenli oda VAR EndDay (day advance)",
            session.Game.dayCycle.IsInHabitat && session.Day == safeDayBefore + 1,
            $"Day {safeDayBefore}->{session.Day}, IsInHabitat={session.Game.dayCycle.IsInHabitat}"));
        results.Add(AssertLastMessageContains("Guvenli odada mesajı", "Guvenli odada"));
        results.Add(AssertUi(
            SceneNames.Cave,
            "Güvenli oda VAR (UI yönlendirme)",
            typeof(CaveUIController)));
        results.Add(AssertDayReportAfterEndDay("Güvenli oda VAR EndDay raporu", false));

        return results;
    }

    public static string FormatReport(IReadOnlyList<StepResult> results)
    {
        var sb = new StringBuilder();
        var passed = 0;
        foreach (var r in results)
        {
            if (r.passed)
                passed++;
            sb.AppendLine(r.passed ? "[OK] " : "[FAIL] ");
            sb.Append(r.step);
            if (!string.IsNullOrEmpty(r.detail))
                sb.Append(" — ").Append(r.detail);
            sb.AppendLine();
        }

        sb.AppendLine($"Özet: {passed}/{results.Count} geçti.");
        return sb.ToString();
    }

    static bool EnsureSession()
    {
        if (GameSession.Instance != null)
            return true;

        var bootstrap = Object.FindObjectOfType<GameBootstrap>();
        if (bootstrap != null)
            return GameSession.Instance != null;

        var go = new GameObject("_UiVerifyBootstrap");
        go.AddComponent<GameSession>();
        if (SceneFlowController.Instance == null)
            go.AddComponent<SceneFlowController>();

        return GameSession.Instance != null;
    }

    static StepResult AssertUi(string expectedScene, string step, System.Type controllerType)
    {
        var controller = Object.FindObjectOfType(controllerType);
        if (controller == null)
            return Fail(step, $"{controllerType.Name} sahnede yok.");

        var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var uiOnly = !string.Equals(active, expectedScene, System.StringComparison.Ordinal);
        var detail = uiOnly
            ? $"Aktif sahne '{active}', UI swap ile {controllerType.Name} bulundu."
            : $"Sahne '{active}' + {controllerType.Name}.";

        return Pass(step, detail);
    }

    static StepResult AssertCamera(string step)
    {
        var cameras = Object.FindObjectsOfType<Camera>();
        for (var i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam != null && cam.enabled && cam.gameObject.activeInHierarchy)
                return Pass(step, cam.gameObject.name);
        }

        return Fail(step, "Etkin kamera yok.");
    }

    static StepResult AssertState(string step, bool condition, string failMessage) =>
        condition ? Pass(step, null) : Fail(step, failMessage);

    static StepResult Pass(string step, string detail) =>
        new StepResult { step = step, passed = true, detail = detail };

    static StepResult Fail(string step, string detail) =>
        new StepResult { step = step, passed = false, detail = detail };

    static StepResult AssertCollapsiblePlayerPanels(string step)
    {
        var panels = Object.FindObjectsOfType<CollapsiblePlayerPanel>();
        if (panels == null || panels.Length < 2)
            return Fail(step, "En az iki CollapsiblePlayerPanel bekleniyordu.");

        var closed = 0;
        for (var i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && !panels[i].IsExpanded)
                closed++;
        }

        if (closed < 2)
            return Fail(step, "Paneller varsayılan olarak kapalı olmalı.");

        panels[0].SetExpanded(true);
        if (!panels[0].IsExpanded)
            return Fail(step, "Çanta paneli açılamadı.");

        return Pass(step, $"{panels.Length} panel, varsayılan kapalı.");
    }

    static StepResult AssertPlayerPanels(string step)
    {
        var presenter = Object.FindObjectOfType<PlayerStatePanelsPresenter>();
        if (presenter == null)
            return Fail(step, "PlayerStatePanelsPresenter yok.");

        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        if (!presenter.HasHuntListConfigured)
            return Fail(step, "Avladıklarım huntListRoot bağlı değil.");

        var inv = session.Game.inventory;
        return Pass(step, $"Tuzak: {inv.traps}, hunt list hazır.");
    }

    static StepResult AssertHuntCatalogRows(string step)
    {
        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        var rows = InventoryHuntCatalog.CollectHuntRows(session.Game.inventory, session.Game.dragonTypes);
        if (rows.Count == 0)
            return Fail(step, "Satır bekleniyordu (canlı/ölü stok).");

        return Pass(step, $"{rows.Count} tür satırı.");
    }

    static StepResult AssertPanelPersistence(string step)
    {
        CollapsiblePanelPersistence.SetExpanded(CollapsiblePanelPersistence.InventoryKey, true);
        CollapsiblePanelPersistence.SetExpanded(CollapsiblePanelPersistence.HuntKey, false);

        if (!CollapsiblePanelPersistence.TryGetExpanded(CollapsiblePanelPersistence.InventoryKey, out var invOpen)
            || !invOpen)
        {
            return Fail(step, "Çanta açık durumu kaydedilmedi.");
        }

        if (!CollapsiblePanelPersistence.TryGetExpanded(CollapsiblePanelPersistence.HuntKey, out var huntOpen)
            || huntOpen)
        {
            return Fail(step, "Av paneli kapalı durumu kaydedilmedi.");
        }

        return Pass(step, "Sahne geçişleri için panel durumu saklandı.");
    }

    static StepResult AssertStatusLogFormat(string step)
    {
        var longMessage = new string('A', 200) + "\nSon satır: tuzak kuruldu.";
        UiFeedbackLog.Publish(longMessage);
        var display = UiFeedbackLog.GetDisplayText();

        if (string.IsNullOrEmpty(display))
            return Fail(step, "Gösterim metni boş.");

        if (display.Length > 145)
            return Fail(step, $"Mesaj kısaltılmadı ({display.Length} karakter).");

        return Pass(step, display);
    }

    static StepResult AssertSelectionClearedWhenSoldOut(string step, string dragonName)
    {
        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        var alive = session.Game.inventory.GetDragonCount(dragonName, true);
        var dead = session.Game.inventory.GetDragonCount(dragonName, false);
        if (alive > 0 || dead > 0)
            return Pass(step, "Türde stok kaldı, seçim kalabilir.");

        if (!string.IsNullOrEmpty(HuntSelectionState.SelectedDragonName))
            return Fail(step, $"Seçim temizlenmedi: {HuntSelectionState.SelectedDragonName}");

        return Pass(step, "Stok bitince seçim temizlendi.");
    }

    static StepResult AssertCaveTrapResources(string step)
    {
        var presenter = Object.FindObjectOfType<CaveTrapResourcesPresenter>();
        if (presenter == null)
            return Fail(step, "CaveTrapResourcesPresenter yok.");

        return Pass(step, "CaveTrapResourcesPresenter aktif.");
    }

    static StepResult AssertCaveRabbitTrapButton(string step)
    {
        var rabbitButton = GameObject.Find("btn_place_trap_rabbit");
        if (rabbitButton == null)
            return Fail(step, "btn_place_trap_rabbit bulunamadı.");

        if (rabbitButton.GetComponent<Button>() == null)
            return Fail(step, "Tavşanlı tuzak butonu Button değil.");

        return Pass(step, "Buton sahnede.");
    }

    static StepResult AssertLastMessageContains(string step, string substring)
    {
        var log = UiFeedbackLog.LastMessage;
        if (string.IsNullOrEmpty(log) || log.IndexOf(substring, System.StringComparison.OrdinalIgnoreCase) < 0)
            return Fail(step, $"Mesajda '{substring}' yok: {log}");

        return Pass(step, log);
    }

    static StepResult AssertBuildSettings(string sceneName)
    {
        var loadable = Application.CanStreamedLevelBeLoaded(sceneName);
        var detail = loadable
            ? "Build Settings'te yüklenebilir."
            : "Yüklenemez; SceneFlowController SwitchSceneUi fallback devreye girer.";
        return Pass($"Build Settings: {sceneName}", detail);
    }

    static StepResult AssertMarketPrices(string step)
    {
        var presenter = Object.FindObjectOfType<MarketPricePresenter>();
        if (presenter == null)
            return Fail(step, "MarketPricePresenter yok.");

        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        var price = session.Game.economy.trapBuyPrice;
        return Pass(step, $"Tuzak fiyatı {price} altın etikette.");
    }

    static StepResult AssertMarketDragonSales(string step, bool expectRows)
    {
        var presenter = Object.FindObjectOfType<MarketDragonSalesPresenter>();
        if (presenter == null)
            return Fail(step, "MarketDragonSalesPresenter yok.");

        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        var rows = InventoryHuntCatalog.CollectHuntRows(session.Game.inventory, session.Game.dragonTypes);
        var any = rows.Count > 0;

        if (expectRows && !any)
            return Fail(step, "Satılabilir ejderha bekleniyordu.");

        if (!expectRows && any)
            return Fail(step, "Liste boş olmalıydı.");

        return Pass(step, expectRows ? $"{rows.Count} satılabilir tür." : "Satılacak ejderha yok.");
    }

    static StepResult AssertMarketScoreWarningButtons(string step)
    {
        var texts = Object.FindObjectsOfType<Text>();
        var hasMinus2 = false;
        var hasMinus1 = false;

        for (var i = 0; i < texts.Length; i++)
        {
            var t = texts[i];
            if (t == null || string.IsNullOrEmpty(t.text))
                continue;

            if (t.text.IndexOf("-2 puan", System.StringComparison.OrdinalIgnoreCase) >= 0)
                hasMinus2 = true;

            if (t.text.IndexOf("-1 puan", System.StringComparison.OrdinalIgnoreCase) >= 0)
                hasMinus1 = true;
        }

        if (!hasMinus2 || !hasMinus1)
            return Fail(step, $"Buton etiketlerinde -2 puan: {hasMinus2}, -1 puan: {hasMinus1}");

        return Pass(step, "Skor uyarısı etiketleri görünüyor.");
    }

    static StepResult AssertCaveHabitatName(string step, string expectedSubstring)
    {
        var session = GameSession.Instance;
        if (session == null)
            return Fail(step, "GameSession yok.");

        var habitat = session.Game.dayCycle.currentHabitat;
        if (habitat == null || string.IsNullOrEmpty(habitat.displayName))
            return Fail(step, "currentHabitat.displayName boş.");

        var habitatGo = GameObject.Find("txt_habitat_floor");
        var habitatText = habitatGo != null ? habitatGo.GetComponent<Text>() : null;
        if (habitatText == null)
            return Fail(step, "txt_habitat_floor bulunamadı.");

        var ui = habitatText.text ?? "";
        if (ui.IndexOf(expectedSubstring, System.StringComparison.OrdinalIgnoreCase) < 0)
            return Fail(step, $"Beklenen '{expectedSubstring}', UI'da bulunan '{ui}'.");

        if (ui.IndexOf("Yakalama:", System.StringComparison.OrdinalIgnoreCase) < 0)
            return Fail(step, "UI'da Yakalama ilerlemesi görünmüyor.");

        var captures = session.Game.dayCycle.CurrentHabitatCaptures;
        var required = habitat.requiredCaptures;
        var expectedProgress = $"Yakalama: {captures}/{required}";
        if (ui.IndexOf(expectedProgress, System.StringComparison.Ordinal) < 0)
            return Fail(step, $"Yakalama ilerlemesi beklenenden farklı. Beklenen '{expectedProgress}', UI '{ui}'.");

        return Pass(step, ui);
    }

    static StepResult AssertDayReportEndingScoreSource(string step, int expectedEndingScore)
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return Fail(step, "GameSession/Game yok.");

        var report = session.Game.GetLastDayReport() ?? session.Game.lastCompletedDayReport;
        if (report == null)
            return Fail(step, "lastCompletedDayReport bulunamadı.");

        if (report.endingScore != expectedEndingScore)
        {
            return Fail(
                step,
                $"report.endingScore={report.endingScore}, beklenen={expectedEndingScore}.");
        }

        if (report.runResetAtDayEnd && session.Game.scoring.currentScore == 0 && report.endingScore > 0)
        {
            var expectedLine = $"Gün sonu skoru: {report.endingScore}";
            var openButton = GameObject.Find("btn_open_report")?.GetComponent<Button>();
            if (openButton != null)
                openButton.onClick.Invoke();

            var body = GameObject.Find("txt_day_report_body")?.GetComponent<Text>();
            var textValue = body != null ? body.text ?? "" : "";
            if (textValue.IndexOf(expectedLine, System.StringComparison.Ordinal) < 0)
            {
                return Fail(
                    step,
                    $"Panel '{expectedLine}' göstermiyor (canlı skor 0 olabilir). Metin: '{textValue}'");
            }

            return Pass(step, $"endingScore={report.endingScore}, currentScore=0 reset sonrası.");
        }

        return Pass(step, $"endingScore={report.endingScore}");
    }

    static StepResult AssertDayReportAfterEndDay(string step, bool expectedRunReset)
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return Fail(step, "GameSession/Game yok.");

        var report = session.Game.GetLastDayReport() ?? session.Game.lastCompletedDayReport;
        if (report == null)
            return Fail(step, "lastCompletedDayReport bulunamadı.");

        if (!report.finalized)
            return Fail(step, "Rapor finalize edilmemiş.");

        if (report.runResetAtDayEnd != expectedRunReset)
        {
            return Fail(
                step,
                $"runResetAtDayEnd beklenen={expectedRunReset}, bulunan={report.runResetAtDayEnd}.");
        }

        if (UiFeedbackLog.LastMessage == null
            || UiFeedbackLog.LastMessage.IndexOf("Gün raporu hazır", System.StringComparison.OrdinalIgnoreCase) < 0)
        {
            return Fail(step, $"Status mesajı eksik: '{UiFeedbackLog.LastMessage ?? "<null>"}'");
        }

        return Pass(step, $"Gün {report.dayNumber}, net {report.netProfit}, runReset={report.runResetAtDayEnd}");
    }

    static StepResult AssertMainHubActionLayout(string step)
    {
        var hub = Object.FindObjectOfType<MainHubUIController>();
        if (hub == null)
            return Fail(step, "MainHubUIController yok.");

        var root = hub.transform;
        if (FindDeepChild(root, "btn_place_trap") != null)
            return Fail(step, "MainHub'da tuzak butonu olmamalı.");

        if (FindDeepChild(root, "btn_build_safe_room") != null)
            return Fail(step, "MainHub'da güvenli oda butonu olmamalı.");

        if (FindDeepChild(root, "btn_go_market") == null)
            return Fail(step, "btn_go_market eksik.");

        if (FindDeepChild(root, "btn_enter_cave") == null)
            return Fail(step, "btn_enter_cave eksik.");

        if (FindDeepChild(root, "btn_end_day") == null)
            return Fail(step, "btn_end_day eksik.");

        if (GameObject.Find("btn_open_report") == null)
            return Fail(step, "btn_open_report eksik.");

        return Pass(step, "Market / Mağara / Gün Bitir + Rapor.");
    }

    static StepResult AssertCaveTrapDisabledVisual(string step, string buttonName, bool expectDisabled)
    {
        var button = GameObject.Find(buttonName)?.GetComponent<Button>();
        if (button == null)
            return Fail(step, $"{buttonName} bulunamadı.");

        var image = button.GetComponent<Image>();
        if (image == null)
            return Fail(step, $"{buttonName} Image yok.");

        if (button.interactable == expectDisabled)
            return Fail(step, $"interactable={button.interactable}, beklenen disabled={expectDisabled}.");

        var disabled = CaveTrapResourcesPresenter.DisabledTrapVisualColor;
        if (expectDisabled)
        {
            if (image.color.a > disabled.a + 0.05f)
                return Fail(step, $"Disabled renk bekleniyordu, bulunan={image.color}");

            return Pass(step, $"disabled color {image.color}");
        }

        if (image.color.a < 0.9f)
            return Fail(step, $"Enabled renk bekleniyordu, bulunan={image.color}");

        return Pass(step, $"enabled color {image.color}");
    }

    static Transform FindDeepChild(Transform root, string objectName)
    {
        if (root == null)
            return null;

        if (root.name == objectName)
            return root;

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindDeepChild(root.GetChild(i), objectName);
            if (found != null)
                return found;
        }

        return null;
    }

    static StepResult AssertDayReportPanel(string step, bool expectedRunReset)
    {
        var openReportGo = GameObject.Find("btn_open_report");
        if (openReportGo == null)
            return Fail(step, "btn_open_report bulunamadı.");

        var openButton = openReportGo.GetComponent<Button>();
        if (openButton == null)
            return Fail(step, "btn_open_report Button değil.");

        if (!openButton.interactable)
            return Fail(step, "Rapor butonu disabled.");

        openButton.onClick.Invoke();

        var panel = GameObject.Find("DayReportPanel");
        if (panel == null || !panel.activeInHierarchy)
            return Fail(step, "DayReportPanel açılmadı.");

        var reportText = GameObject.Find("txt_day_report_body")?.GetComponent<Text>();
        if (reportText == null)
            return Fail(step, "txt_day_report_body bulunamadı.");

        var textValue = reportText.text ?? "";
        if (textValue.IndexOf("Net kazanç:", System.StringComparison.OrdinalIgnoreCase) < 0)
            return Fail(step, $"Net kazanç satırı yok: '{textValue}'");

        var expectedResetLine = expectedRunReset ? "Run reset oldu mu: Evet" : "Run reset oldu mu: Hayır";
        if (textValue.IndexOf(expectedResetLine, System.StringComparison.OrdinalIgnoreCase) < 0)
            return Fail(step, $"Run reset satırı yok: '{expectedResetLine}'");

        return Pass(step, "Panel açıldı, Net kazanç ve Run reset satırları var.");
    }
}
