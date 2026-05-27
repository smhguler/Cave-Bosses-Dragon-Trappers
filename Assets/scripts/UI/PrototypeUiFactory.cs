using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Prototip UI hiyerarşisini kod ile kurar (Editor ve runtime paylaşımlı).
/// </summary>
public static class PrototypeUiFactory
{
    public static void EnsureCamera()
    {
        Camera chosen = null;
        var cameras = Object.FindObjectsOfType<Camera>();

        for (var i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam == null || !cam.gameObject.activeInHierarchy)
                continue;

            if (cam.CompareTag("MainCamera"))
            {
                chosen = cam;
                break;
            }

            if (chosen == null && cam.enabled)
                chosen = cam;
        }

        if (chosen == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            chosen = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            chosen.orthographic = true;
            chosen.orthographicSize = 5f;
            chosen.clearFlags = CameraClearFlags.SolidColor;
            chosen.backgroundColor = new Color(0.08f, 0.1f, 0.12f, 1f);
            chosen.nearClipPlane = 0.3f;
            chosen.farClipPlane = 1000f;
            chosen.depth = -1;
        }

        chosen.enabled = true;
        chosen.gameObject.tag = "MainCamera";

        for (var i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam == null || cam == chosen)
                continue;

            cam.enabled = false;
        }
    }

    public static void EnsureSceneUi(string sceneName)
    {
        EnsureCamera();

        switch (sceneName)
        {
            case SceneNames.MainHub:
                if (NeedsPrototypeUiRebuild<MainHubUIController>())
                {
                    DestroyController<MainHubUIController>();
                    BuildMainHub();
                }

                break;
            case SceneNames.Market:
                if (NeedsPrototypeUiRebuild<MarketUIController>())
                {
                    DestroyController<MarketUIController>();
                    BuildMarket();
                }

                break;
            case SceneNames.Cave:
                if (NeedsPrototypeUiRebuild<CaveUIController>())
                {
                    DestroyController<CaveUIController>();
                    BuildCave();
                }

                break;
            default:
                if (NeedsPrototypeUiRebuild<MainHubUIController>())
                {
                    DestroyController<MainHubUIController>();
                    BuildMainHub();
                }

                break;
        }
    }

    static bool NeedsPrototypeUiRebuild<T>() where T : Component
    {
        if (Object.FindObjectOfType<T>() == null)
            return true;

        if (Object.FindObjectsOfType<CollapsiblePlayerPanel>().Length < 2)
            return true;

        var presenter = Object.FindObjectOfType<PlayerStatePanelsPresenter>();
        return presenter == null || !presenter.HasHuntListConfigured;
    }

    public static void SwitchSceneUi(string sceneName)
    {
        ClearPrototypeUi();
        EnsureCamera();
        EnsureEventSystem();
        EnsureSceneUi(sceneName);
    }

    static void ClearPrototypeUi()
    {
        DestroyController<MainHubUIController>();
        DestroyController<MarketUIController>();
        DestroyController<CaveUIController>();
    }

    static void DestroyController<T>() where T : Component
    {
        var controller = Object.FindObjectOfType<T>();
        if (controller == null)
            return;

        var root = controller.gameObject;
        if (Application.isPlaying)
            Object.Destroy(root);
        else
            Object.DestroyImmediate(root);
    }

    public static void BuildMainHub() => BuildMainHubInternal();
    public static void BuildMarket() => BuildMarketInternal();
    public static void BuildCave() => BuildCaveInternal();

    static void BuildMainHubInternal()
    {
        EnsureEventSystem();
        var canvas = CreateCanvas("MainHubCanvas");
        BuildMainHubVisuals(canvas.transform);
        var hud = CreateHud(canvas.transform);
        CreateTitle(canvas.transform, "Dragon Trappers");
        CreateSubtitle(canvas.transform, "Firma merkezi", new Vector2(0.5f, 0.735f));
        CreateMainHubPreview(canvas.transform);

        var actions = CreateVerticalButtonPanel(
            canvas.transform,
            new[]
            {
                ("Market'e Git", "btn_go_market"),
                ("Mağaraya Gir", "btn_enter_cave"),
                ("Gün Bitir", "btn_end_day"),
            },
            new Vector2(0.5f, 0.36f),
            new Vector2(380f, 240f));
        var reportUi = CreateDayReportUi(canvas.transform);

        CreateActionIconStrip(canvas.transform, new Vector2(0.5f, 0.18f));
        var status = CreateStatusText(canvas.transform);
        var hub = canvas.gameObject.AddComponent<MainHubUIController>();
        hub.Configure(actions[0], actions[1], actions[2], status);
        BindHud(hud, canvas.gameObject);
        AttachPlayerStatePanels(canvas.gameObject, canvas.transform);
        AttachStatusLog(canvas.gameObject, status);
        AttachDayReportPanel(canvas.gameObject, reportUi.openButton, reportUi.openLabel, reportUi.panelRoot, reportUi.bodyText, reportUi.closeButton);

        AttachAllCavesCompletedWinPanel(canvas.transform, canvas.gameObject);
    }

    static void BuildMarketInternal()
    {
        EnsureEventSystem();
        var canvas = CreateCanvas("MarketCanvas");
        BuildMarketVisuals(canvas.transform);
        var hud = CreateHud(canvas.transform);
        CreateTitle(canvas.transform, "Market");
        CreateSubtitle(canvas.transform, "Alış ve ejderha satışı", new Vector2(0.5f, 0.735f));

        var buyActions = CreateVerticalButtonPanel(
            canvas.transform,
            new[]
            {
                ("Tuzak Al", "btn_buy_trap"),
                ("Tavşan Al", "btn_buy_rabbit"),
                ("Ana Ekrana Dön", "btn_back"),
            },
            new Vector2(0.28f, 0.36f),
            new Vector2(320f, 220f));
        var reportUi = CreateDayReportUi(canvas.transform);

        CreateDecorLabel(canvas.transform, new Vector2(0.28f, 0.52f), "Malzeme alışı");

        var dragonSales = CreateMarketDragonSalesSection(canvas.transform);
        var status = CreateStatusText(canvas.transform);
        var market = canvas.gameObject.AddComponent<MarketUIController>();
        market.Configure(buyActions[0], buyActions[1], buyActions[2], status);
        AttachMarketPrices(canvas.gameObject, buyActions[0], buyActions[1]);
        AttachMarketDragonSales(canvas.gameObject, dragonSales.listRoot, dragonSales.emptyLabel);
        BindHud(hud, canvas.gameObject);
        AttachPlayerStatePanels(canvas.gameObject, canvas.transform);
        AttachStatusLog(canvas.gameObject, status);
        AttachDayReportPanel(canvas.gameObject, reportUi.openButton, reportUi.openLabel, reportUi.panelRoot, reportUi.bodyText, reportUi.closeButton);
    }

    static void BuildCaveInternal()
    {
        EnsureEventSystem();
        var canvas = CreateCanvas("CaveCanvas");
        BuildCaveVisuals(canvas.transform);
        var hud = CreateHud(canvas.transform);
        CreateTitle(canvas.transform, "Mağara");
        CreateSubtitle(canvas.transform, "Tuzak hattı", new Vector2(0.5f, 0.735f));
        var habitatFloor = CreateCaveHabitatLabel(canvas.transform);
        CreateCaveEncounterPreview(canvas.transform);

        var trapActions = CreateCaveTrapActionStrip(canvas.transform);
        var actions = CreateVerticalButtonPanel(
            canvas.transform,
            new[]
            {
                ("Güvenli Oda Kur", "btn_build_safe_room"),
                ("Daha Derine İn", "btn_travel_deeper"),
                ("Gün Bitir", "btn_end_day"),
                ("Ana Merkez'e Dön", "btn_return_hq"),
            },
            new Vector2(0.5f, 0.22f),
            new Vector2(400f, 300f));
        var reportUi = CreateDayReportUi(canvas.transform);

        var status = CreateStatusText(canvas.transform);
        var cave = canvas.gameObject.AddComponent<CaveUIController>();
        cave.Configure(
            trapActions.placeTrap,
            trapActions.placeTrapWithRabbit,
            actions[0],
            actions[1],
            actions[2],
            actions[3],
            status);
        AttachCaveTrapResources(
            canvas.gameObject,
            trapActions.placeTrap,
            trapActions.placeTrapWithRabbit,
            trapActions.trapStockLabel,
            trapActions.rabbitStockLabel,
            trapActions.rabbitBonusLabel);
        AttachCaveHabitatLabel(canvas.gameObject, habitatFloor);
        BindHud(hud, canvas.gameObject);
        AttachPlayerStatePanels(canvas.gameObject, canvas.transform);
        AttachStatusLog(canvas.gameObject, status);
        AttachDayReportPanel(canvas.gameObject, reportUi.openButton, reportUi.openLabel, reportUi.panelRoot, reportUi.bodyText, reportUi.closeButton);
        AttachAllCavesCompletedWinPanel(canvas.transform, canvas.gameObject);
    }

    static void AttachAllCavesCompletedWinPanel(Transform canvasRoot, GameObject host)
    {
        var panel = CreateVisualCard(
            canvasRoot,
            "AllCavesCompletedPanel",
            new Vector2(0.5f, 0.52f),
            new Vector2(820f, 260f),
            new Color(0.06f, 0.12f, 0.08f, 0.92f));

        var rect = panel.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        panel.SetActive(false);

        var titleGo = CreateUiObject("txt_allcaves_completed_title", panel.transform);
        var titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(20f, -20f);
        titleRect.offsetMax = new Vector2(-20f, 0f);

        var titleText = titleGo.AddComponent<Text>();
        ApplyFont(titleText);
        titleText.fontSize = 30;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.58f, 1f, 0.48f);
        titleText.raycastTarget = false;
        titleText.supportRichText = true;
        titleText.text = "Tüm mağaralar tamamlandı!\n<b>Kazandın.</b>";

        host.AddComponent<CaveAllCavesCompletedPresenter>().Configure(panel);
    }

    static void BuildMainHubVisuals(Transform parent)
    {
        CreateBackground(parent, new Color(0.07f, 0.11f, 0.13f), new Color(0.12f, 0.18f, 0.16f));
        CreateBand(parent, "back_wall", new Color(0.15f, 0.22f, 0.21f), new Vector2(0.5f, 0.58f), new Vector2(1500f, 560f));
        CreateBand(parent, "floor", new Color(0.20f, 0.16f, 0.12f), new Vector2(0.5f, 0.11f), new Vector2(1700f, 230f));
        CreateLowPolyGem(parent, "banner_left", new Vector2(0.19f, 0.68f), 130f, new Color(0.75f, 0.25f, 0.18f), new Color(1f, 0.73f, 0.25f));
        CreateLowPolyGem(parent, "banner_right", new Vector2(0.81f, 0.68f), 130f, new Color(0.25f, 0.49f, 0.42f), new Color(0.85f, 0.92f, 0.55f));
        CreateVisualCard(parent, "notice_board", new Vector2(0.22f, 0.42f), new Vector2(360f, 310f), new Color(0.28f, 0.19f, 0.12f, 0.94f));
        CreateBoardLines(parent, new Vector2(0.22f, 0.42f));
        CreateVisualCard(parent, "dragon_license", new Vector2(0.78f, 0.42f), new Vector2(360f, 310f), new Color(0.10f, 0.19f, 0.21f, 0.94f));
        CreateDragonSilhouette(parent, new Vector2(0.78f, 0.43f), 1.05f, new Color(0.92f, 0.41f, 0.22f, 0.92f));
    }

    static void BuildMarketVisuals(Transform parent)
    {
        CreateBackground(parent, new Color(0.10f, 0.14f, 0.13f), new Color(0.23f, 0.16f, 0.10f));
        CreateBand(parent, "market_wall", new Color(0.24f, 0.20f, 0.14f), new Vector2(0.5f, 0.56f), new Vector2(1600f, 530f));
        CreateBand(parent, "market_counter", new Color(0.34f, 0.19f, 0.10f), new Vector2(0.5f, 0.27f), new Vector2(1250f, 170f));
        CreateAwning(parent, new Vector2(0.32f, 0.66f), new Color(0.75f, 0.19f, 0.16f), new Color(0.92f, 0.76f, 0.42f));
        CreateAwning(parent, new Vector2(0.68f, 0.66f), new Color(0.18f, 0.43f, 0.46f), new Color(0.92f, 0.76f, 0.42f));
        CreateDecorLabel(parent, new Vector2(0.24f, 0.52f), "Tuzak tezgahı");
        CreateDecorLabel(parent, new Vector2(0.76f, 0.52f), "Ejderha borsası");
        CreateSpriteAsset(parent, "rabbit_asset_market", "ThirdPartyVisuals/rabbit_kenney", new Vector2(0.39f, 0.52f), new Vector2(100f, 100f));
        CreateSpriteAsset(parent, "trap_asset_market", "ThirdPartyVisuals/trap_bear", new Vector2(0.24f, 0.58f), new Vector2(96f, 96f));
    }

    static void BuildCaveVisuals(Transform parent)
    {
        CreateBackground(parent, new Color(0.05f, 0.07f, 0.08f), new Color(0.09f, 0.11f, 0.13f));
        CreateBand(parent, "cave_depth", new Color(0.10f, 0.13f, 0.16f), new Vector2(0.5f, 0.52f), new Vector2(1600f, 610f));
        CreateBand(parent, "cave_floor", new Color(0.12f, 0.10f, 0.09f), new Vector2(0.5f, 0.13f), new Vector2(1700f, 250f));
        CreateStalactites(parent);
        CreateRockCluster(parent, new Vector2(0.19f, 0.25f), 1.1f);
        CreateRockCluster(parent, new Vector2(0.82f, 0.24f), 1.25f);
        CreateSafeRoom(parent, new Vector2(0.78f, 0.49f));
        CreateTrapIcon(parent, new Vector2(0.37f, 0.42f), 1.25f, new Color(0.72f, 0.76f, 0.69f));
        CreateSpriteAsset(parent, "trap_asset_cave", "ThirdPartyVisuals/trap_bear", new Vector2(0.37f, 0.45f), new Vector2(120f, 120f));
        CreateSpriteAsset(parent, "rabbit_asset_cave", "ThirdPartyVisuals/rabbit_kenney", new Vector2(0.27f, 0.38f), new Vector2(92f, 92f));
        CreateDragonSilhouette(parent, new Vector2(0.63f, 0.48f), 1.2f, new Color(0.70f, 0.20f, 0.16f, 0.42f));
        CreateSpriteAsset(parent, "dragon_asset_cave", "ThirdPartyVisuals/dragon_pisilohe10", new Vector2(0.64f, 0.52f), new Vector2(230f, 230f));
    }

    static void CreateMainHubPreview(Transform parent)
    {
        CreateToken(parent, new Vector2(0.22f, 0.52f), "HQ", new Color(0.87f, 0.70f, 0.36f));
        CreateToken(parent, new Vector2(0.22f, 0.41f), "MAP", new Color(0.36f, 0.61f, 0.62f));
        CreateToken(parent, new Vector2(0.22f, 0.30f), "LEDGER", new Color(0.70f, 0.50f, 0.32f));
    }

    static void CreateDecorLabel(Transform parent, Vector2 anchor, string label)
    {
        var go = CreateUiObject("decor_" + label, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 28f);

        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 15;
        text.fontStyle = FontStyle.Italic;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.72f, 0.76f, 0.74f, 0.85f);
        text.text = label;
        text.raycastTarget = false;
    }

    static void CreateCaveEncounterPreview(Transform parent)
    {
        CreateToken(parent, new Vector2(0.37f, 0.57f), "Tuzak", new Color(0.68f, 0.70f, 0.62f));
        CreateToken(parent, new Vector2(0.63f, 0.62f), "Ejderha", new Color(0.80f, 0.28f, 0.20f));
        CreateToken(parent, new Vector2(0.78f, 0.62f), "Güvenli Oda", new Color(0.47f, 0.64f, 0.72f));
    }

    static void CreateBackground(Transform parent, Color top, Color bottom)
    {
        var root = CreateUiObject("VisualBackground", parent);
        var rect = root.GetComponent<RectTransform>();
        StretchFull(rect);
        root.transform.SetAsFirstSibling();

        var upper = CreateUiObject("upper_tone", root.transform);
        var upperRect = upper.GetComponent<RectTransform>();
        upperRect.anchorMin = new Vector2(0f, 0.45f);
        upperRect.anchorMax = new Vector2(1f, 1f);
        upperRect.offsetMin = Vector2.zero;
        upperRect.offsetMax = Vector2.zero;
        upper.AddComponent<Image>().color = top;

        var lower = CreateUiObject("lower_tone", root.transform);
        var lowerRect = lower.GetComponent<RectTransform>();
        lowerRect.anchorMin = Vector2.zero;
        lowerRect.anchorMax = new Vector2(1f, 0.45f);
        lowerRect.offsetMin = Vector2.zero;
        lowerRect.offsetMax = Vector2.zero;
        lower.AddComponent<Image>().color = bottom;
    }

    static GameObject CreateBand(Transform parent, string name, Color color, Vector2 anchor, Vector2 size)
    {
        var go = CreateUiObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    static GameObject CreateVisualCard(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
    {
        var card = CreateBand(parent, name, color, anchor, size);
        var outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
        outline.effectDistance = new Vector2(3f, -3f);
        return card;
    }

    static void CreateBoardLines(Transform parent, Vector2 center)
    {
        for (var i = 0; i < 4; i++)
        {
            var y = center.y + 0.08f - i * 0.055f;
            CreateBand(parent, "board_line_" + i, new Color(0.86f, 0.65f, 0.38f, 0.72f), new Vector2(center.x, y), new Vector2(260f - i * 24f, 10f));
        }
    }

    static void CreateAwning(Transform parent, Vector2 anchor, Color primary, Color secondary)
    {
        CreateBand(parent, "awning_back", new Color(0.10f, 0.08f, 0.06f, 0.35f), anchor + new Vector2(0.01f, -0.01f), new Vector2(410f, 90f));
        for (var i = 0; i < 5; i++)
        {
            var x = anchor.x - 0.085f + i * 0.042f;
            var color = i % 2 == 0 ? primary : secondary;
            CreateBand(parent, "awning_strip_" + i, color, new Vector2(x, anchor.y), new Vector2(76f, 92f));
        }
    }

    static void CreateCrate(Transform parent, Vector2 anchor, string label)
    {
        CreateVisualCard(parent, "crate_" + label, anchor, new Vector2(150f, 92f), new Color(0.36f, 0.22f, 0.12f, 0.96f));
        CreateBand(parent, "crate_slat_" + label, new Color(0.58f, 0.37f, 0.20f, 0.92f), anchor, new Vector2(128f, 12f));
        CreateToken(parent, anchor + new Vector2(0f, -0.005f), label, new Color(0.78f, 0.58f, 0.36f));
    }

    static void CreateStalactites(Transform parent)
    {
        for (var i = 0; i < 7; i++)
        {
            var x = 0.14f + i * 0.12f;
            var height = 95f + (i % 3) * 35f;
            var rock = CreateBand(parent, "stalactite_" + i, new Color(0.16f, 0.19f, 0.22f), new Vector2(x, 0.79f), new Vector2(42f, height));
            rock.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 8f : -8f);
        }
    }

    static void CreateRockCluster(Transform parent, Vector2 anchor, float scale)
    {
        CreateBand(parent, "rock_a", new Color(0.20f, 0.22f, 0.23f), anchor, new Vector2(130f * scale, 78f * scale));
        CreateBand(parent, "rock_b", new Color(0.15f, 0.17f, 0.18f), anchor + new Vector2(0.045f, 0.03f), new Vector2(110f * scale, 108f * scale));
        CreateBand(parent, "rock_c", new Color(0.26f, 0.25f, 0.23f), anchor + new Vector2(-0.04f, 0.02f), new Vector2(86f * scale, 90f * scale));
    }

    static void CreateSafeRoom(Transform parent, Vector2 anchor)
    {
        CreateVisualCard(parent, "safe_room", anchor, new Vector2(190f, 135f), new Color(0.20f, 0.31f, 0.34f, 0.96f));
        CreateBand(parent, "safe_room_roof", new Color(0.36f, 0.46f, 0.48f), anchor + new Vector2(0f, 0.07f), new Vector2(220f, 36f));
        CreateBand(parent, "safe_room_door", new Color(0.10f, 0.14f, 0.15f), anchor + new Vector2(0f, -0.035f), new Vector2(58f, 80f));
        CreateBand(parent, "safe_room_light", new Color(0.95f, 0.74f, 0.36f, 0.85f), anchor + new Vector2(0.045f, 0.012f), new Vector2(26f, 26f));
    }

    static void CreateTrapIcon(Transform parent, Vector2 anchor, float scale, Color color)
    {
        CreateBand(parent, "trap_base", color, anchor, new Vector2(110f * scale, 24f * scale));
        CreateBand(parent, "trap_left", color * 0.9f, anchor + new Vector2(-0.035f * scale, 0.035f * scale), new Vector2(28f * scale, 78f * scale));
        CreateBand(parent, "trap_right", color * 0.9f, anchor + new Vector2(0.035f * scale, 0.035f * scale), new Vector2(28f * scale, 78f * scale));
        CreateBand(parent, "trap_bait", new Color(0.85f, 0.47f, 0.25f), anchor + new Vector2(0f, 0.055f * scale), new Vector2(38f * scale, 28f * scale));
    }

    static void CreateRabbitIcon(Transform parent, Vector2 anchor, float scale, Color color)
    {
        CreateBand(parent, "rabbit_body", color, anchor, new Vector2(82f * scale, 52f * scale));
        CreateBand(parent, "rabbit_head", color * 1.04f, anchor + new Vector2(0.035f * scale, 0.035f * scale), new Vector2(52f * scale, 46f * scale));
        CreateBand(parent, "rabbit_ear_a", color * 1.06f, anchor + new Vector2(0.02f * scale, 0.09f * scale), new Vector2(18f * scale, 68f * scale));
        CreateBand(parent, "rabbit_ear_b", color * 0.96f, anchor + new Vector2(0.06f * scale, 0.09f * scale), new Vector2(18f * scale, 68f * scale));
        CreateBand(parent, "rabbit_eye", new Color(0.08f, 0.06f, 0.05f), anchor + new Vector2(0.052f * scale, 0.043f * scale), new Vector2(10f * scale, 10f * scale));
    }

    static void CreateDragonSilhouette(Transform parent, Vector2 anchor, float scale, Color color)
    {
        CreateBand(parent, "dragon_body", color, anchor, new Vector2(180f * scale, 74f * scale));
        CreateBand(parent, "dragon_neck", color * 0.95f, anchor + new Vector2(0.075f * scale, 0.055f * scale), new Vector2(52f * scale, 120f * scale));
        CreateBand(parent, "dragon_head", color * 1.08f, anchor + new Vector2(0.12f * scale, 0.105f * scale), new Vector2(92f * scale, 62f * scale));
        CreateBand(parent, "dragon_wing_a", color * 0.82f, anchor + new Vector2(-0.055f * scale, 0.095f * scale), new Vector2(130f * scale, 90f * scale));
        CreateBand(parent, "dragon_wing_b", color * 0.72f, anchor + new Vector2(-0.14f * scale, 0.05f * scale), new Vector2(98f * scale, 72f * scale));
        CreateBand(parent, "dragon_tail", color * 0.88f, anchor + new Vector2(-0.15f * scale, -0.015f * scale), new Vector2(130f * scale, 30f * scale));
        CreateBand(parent, "dragon_eye", new Color(1f, 0.87f, 0.28f), anchor + new Vector2(0.14f * scale, 0.112f * scale), new Vector2(12f * scale, 12f * scale));
    }

    static void CreateLowPolyGem(Transform parent, string name, Vector2 anchor, float size, Color a, Color b)
    {
        var left = CreateBand(parent, name + "_left", a, anchor + new Vector2(-0.025f, 0f), new Vector2(size, size));
        left.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        var right = CreateBand(parent, name + "_right", b, anchor + new Vector2(0.025f, 0f), new Vector2(size * 0.74f, size * 0.74f));
        right.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    static void CreateActionIconStrip(Transform parent, Vector2 anchor)
    {
        CreateToken(parent, anchor + new Vector2(-0.16f, 0f), "TRAP", new Color(0.68f, 0.70f, 0.62f));
        CreateToken(parent, anchor + new Vector2(-0.05f, 0f), "RABBIT", new Color(0.84f, 0.78f, 0.65f));
        CreateToken(parent, anchor + new Vector2(0.06f, 0f), "DRAGON", new Color(0.84f, 0.30f, 0.22f));
        CreateToken(parent, anchor + new Vector2(0.18f, 0f), "ROOM", new Color(0.48f, 0.66f, 0.72f));
    }

    static void CreateToken(Transform parent, Vector2 anchor, string label, Color color)
    {
        var token = CreateVisualCard(parent, "token_" + label, anchor, new Vector2(135f, 50f), new Color(color.r, color.g, color.b, 0.92f));
        var textGo = CreateUiObject("label", token.transform);
        StretchFull(textGo.GetComponent<RectTransform>());
        var text = textGo.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 16;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.08f, 0.08f, 0.07f);
        text.text = label;
    }


    static void CreateSpriteAsset(Transform parent, string name, string resourcePath, Vector2 anchor, Vector2 size)
    {
        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            CreateToken(parent, anchor, "asset?", new Color(0.72f, 0.38f, 0.32f));
            return;
        }

        var go = CreateUiObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        var image = go.AddComponent<Image>();
        image.preserveAspect = true;
        image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    static void BindHud((Text day, Text time, Text gold) hud, GameObject host)
    {
        var presenter = host.GetComponent<HudPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<HudPresenter>();
        presenter.Configure(hud.day, hud.time, hud.gold);
    }

    static void AttachPlayerStatePanels(GameObject host, Transform canvasRoot)
    {
        var inventory = CreateCollapsibleSidePanel(
            canvasRoot,
            "InventoryPanel",
            "Çanta",
            new Vector2(0.02f, 0.14f),
            new Vector2(0.26f, 0.88f),
            alignLeft: true);

        var hunt = CreateCollapsibleSidePanel(
            canvasRoot,
            "HuntPanel",
            "Avladıklarım",
            new Vector2(0.74f, 0.14f),
            new Vector2(0.98f, 0.88f),
            alignLeft: false,
            withHuntList: true);

        var presenter = host.GetComponent<PlayerStatePanelsPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<PlayerStatePanelsPresenter>();
        presenter.Configure(
            inventory.panel,
            inventory.bodyText,
            hunt.panel,
            hunt.huntListRoot,
            hunt.huntEmptyText);
    }

    static void AttachStatusLog(GameObject host, Text statusText)
    {
        var log = host.GetComponent<StatusLogPresenter>();
        if (log == null)
            log = host.AddComponent<StatusLogPresenter>();
        log.Configure(statusText);
    }

    static void AttachMarketPrices(GameObject host, Button buyTrap, Button buyRabbit)
    {
        var presenter = host.GetComponent<MarketPricePresenter>();
        if (presenter == null)
            presenter = host.AddComponent<MarketPricePresenter>();
        presenter.Configure(buyTrap, buyRabbit);
    }

    static Text CreateCaveHabitatLabel(Transform parent)
    {
        var go = CreateUiObject("txt_habitat_floor", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.66f);
        rect.anchorMax = new Vector2(0.5f, 0.66f);
        rect.sizeDelta = new Vector2(520f, 96f);

        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 22;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.78f, 0.88f, 0.92f);
        text.text = "Kat: —";
        text.raycastTarget = false;
        return text;
    }

    static void AttachCaveHabitatLabel(GameObject host, Text habitatFloor)
    {
        var presenter = host.GetComponent<CaveHabitatPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<CaveHabitatPresenter>();
        presenter.Configure(habitatFloor);
    }

    static (RectTransform listRoot, Text emptyLabel) CreateMarketDragonSalesSection(Transform parent)
    {
        var section = CreateVisualCard(
            parent,
            "DragonSalesSection",
            new Vector2(0.72f, 0.36f),
            new Vector2(520f, 420f),
            new Color(0.05f, 0.08f, 0.09f, 0.9f));

        var sectionRect = section.GetComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0.54f, 0.14f);
        sectionRect.anchorMax = new Vector2(0.98f, 0.88f);
        sectionRect.pivot = new Vector2(0.5f, 0.5f);
        sectionRect.offsetMin = Vector2.zero;
        sectionRect.offsetMax = Vector2.zero;

        var titleGo = CreateUiObject("dragon_sales_title", section.transform);
        var titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 36f);
        titleRect.anchoredPosition = Vector2.zero;
        var titleText = titleGo.AddComponent<Text>();
        ApplyFont(titleText);
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.95f, 0.78f, 0.42f);
        titleText.text = "Ejderha Satışı";
        titleText.raycastTarget = false;

        var emptyGo = CreateUiObject("dragon_sales_empty", section.transform);
        var emptyRect = emptyGo.GetComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0f, 0f);
        emptyRect.anchorMax = new Vector2(1f, 1f);
        emptyRect.offsetMin = new Vector2(12f, 12f);
        emptyRect.offsetMax = new Vector2(-12f, -40f);
        var emptyText = emptyGo.AddComponent<Text>();
        ApplyFont(emptyText);
        emptyText.fontSize = 18;
        emptyText.alignment = TextAnchor.MiddleCenter;
        emptyText.color = new Color(0.75f, 0.8f, 0.82f);
        emptyText.text = "Satılacak ejderha yok";
        emptyText.raycastTarget = false;

        var listGo = CreateUiObject("dragon_sales_list", section.transform);
        var listRect = listGo.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.offsetMin = new Vector2(10f, 10f);
        listRect.offsetMax = new Vector2(-10f, -42f);

        var layout = listGo.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var fitter = listGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return (listRect, emptyText);
    }

    static void AttachMarketDragonSales(GameObject host, RectTransform listRoot, Text emptyLabel)
    {
        var presenter = host.GetComponent<MarketDragonSalesPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<MarketDragonSalesPresenter>();
        presenter.Configure(listRoot, emptyLabel);
    }

    static void AttachDayReportPanel(
        GameObject host,
        Button openButton,
        Text openLabel,
        GameObject panelRoot,
        Text bodyText,
        Button closeButton)
    {
        var presenter = host.GetComponent<DayReportPanelPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<DayReportPanelPresenter>();
        presenter.Configure(openButton, openLabel, panelRoot, bodyText, closeButton);
    }

    static (Button openButton, Text openLabel, GameObject panelRoot, Text bodyText, Button closeButton) CreateDayReportUi(Transform parent)
    {
        var openButton = CreateButton(parent, "Rapor", "btn_open_report", new Vector2(0f, 0f));
        var openRect = openButton.GetComponent<RectTransform>();
        openRect.anchorMin = new Vector2(1f, 1f);
        openRect.anchorMax = new Vector2(1f, 1f);
        openRect.pivot = new Vector2(1f, 1f);
        openRect.anchoredPosition = new Vector2(-18f, -74f);
        openRect.sizeDelta = new Vector2(170f, 42f);

        var openLabel = openButton.GetComponentInChildren<Text>();
        if (openLabel != null)
            openLabel.fontSize = 18;

        var modalRoot = CreateUiObject("DayReportModal", parent);
        var modalRect = modalRoot.GetComponent<RectTransform>();
        StretchFull(modalRect);
        modalRoot.transform.SetAsLastSibling();

        var overlayGo = CreateUiObject("day_report_overlay", modalRoot.transform);
        StretchFull(overlayGo.GetComponent<RectTransform>());
        var overlayImage = overlayGo.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.94f);
        overlayImage.raycastTarget = true;

        var panel = CreateUiObject("DayReportPanel", modalRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(940f, 720f);

        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.05f, 0.07f, 1f);
        panelImage.raycastTarget = true;

        var panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0f, 0f, 0.65f);
        panelOutline.effectDistance = new Vector2(3f, -3f);

        var titleGo = CreateUiObject("txt_day_report_title", panel.transform);
        var titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(0f, 48f);
        var titleText = titleGo.AddComponent<Text>();
        ApplyFont(titleText);
        titleText.fontSize = 30;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.95f, 0.82f, 0.45f);
        titleText.text = "Gün Sonu / Firma Raporu";
        titleText.raycastTarget = false;

        var closeButton = CreateButton(panel.transform, "Kapat", "btn_close_report", Vector2.zero);
        var closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-18f, -14f);
        closeRect.sizeDelta = new Vector2(128f, 40f);
        closeButton.transform.SetAsLastSibling();

        var scrollGo = CreateUiObject("day_report_scroll", panel.transform);
        var scrollRect = scrollGo.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(36f, 24f);
        scrollRect.offsetMax = new Vector2(-28f, -68f);

        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var viewportGo = CreateUiObject("day_report_viewport", scrollGo.transform);
        var viewportRect = viewportGo.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        var viewportImage = viewportGo.AddComponent<Image>();
        viewportImage.color = new Color(0.02f, 0.04f, 0.05f, 1f);
        viewportGo.AddComponent<Mask>().showMaskGraphic = false;

        var contentGo = CreateUiObject("day_report_content", viewportGo.transform);
        var contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var bodyGo = CreateUiObject("txt_day_report_body", contentGo.transform);
        var bodyRect = bodyGo.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(0f, 0f);

        var bodyLe = bodyGo.AddComponent<LayoutElement>();
        bodyLe.flexibleWidth = 1f;
        bodyLe.minWidth = 400f;

        var bodyText = bodyGo.AddComponent<Text>();
        ApplyFont(bodyText);
        bodyText.fontSize = 19;
        bodyText.lineSpacing = 1.15f;
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.color = new Color(0.92f, 0.95f, 0.96f);
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.raycastTarget = false;
        bodyText.supportRichText = true;
        bodyText.text = "Henüz rapor yok.";

        scroll.content = contentRect;
        scroll.viewport = viewportRect;

        modalRoot.SetActive(false);

        return (openButton, openLabel, modalRoot, bodyText, closeButton);
    }

    static (
        CollapsiblePlayerPanel panel,
        Text bodyText,
        RectTransform huntListRoot,
        Text huntEmptyText)
        CreateCollapsibleSidePanel(
            Transform parent,
            string panelName,
            string title,
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool alignLeft,
            bool withHuntList = false)
    {
        var panelGo = CreateVisualCard(
            parent,
            panelName,
            Vector2.Lerp(anchorMin, anchorMax, 0.5f),
            Vector2.zero,
            new Color(0.04f, 0.06f, 0.07f, 0.88f));
        var panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.anchorMin = anchorMin;
        panelRoot.anchorMax = anchorMax;
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.offsetMin = Vector2.zero;
        panelRoot.offsetMax = Vector2.zero;

        var toggleGo = CreateUiObject("panel_toggle", panelGo.transform);
        var toggleRect = toggleGo.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(0.5f, 1f);
        toggleRect.sizeDelta = new Vector2(0f, 40f);
        toggleRect.anchoredPosition = Vector2.zero;

        var toggleImage = toggleGo.AddComponent<Image>();
        toggleImage.color = new Color(0.18f, 0.28f, 0.3f, 0.98f);
        var toggleButton = toggleGo.AddComponent<Button>();

        var toggleTextGo = CreateUiObject("Text", toggleGo.transform);
        StretchFull(toggleTextGo.GetComponent<RectTransform>());
        var toggleText = toggleTextGo.AddComponent<Text>();
        ApplyFont(toggleText);
        toggleText.fontSize = 16;
        toggleText.fontStyle = FontStyle.Bold;
        toggleText.alignment = TextAnchor.MiddleCenter;
        toggleText.color = new Color(0.97f, 0.98f, 0.94f);
        toggleText.text = title;

        var contentGo = CreateUiObject("panel_content", panelGo.transform);
        var contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(8f, 8f);
        contentRect.offsetMax = new Vector2(-8f, -44f);

        Text bodyText = null;
        RectTransform huntListRoot = null;
        Text huntEmptyText = null;

        if (withHuntList)
        {
            var emptyGo = CreateUiObject("hunt_empty", contentGo.transform);
            StretchFull(emptyGo.GetComponent<RectTransform>());
            huntEmptyText = emptyGo.AddComponent<Text>();
            ApplyFont(huntEmptyText);
            huntEmptyText.fontSize = 15;
            huntEmptyText.alignment = TextAnchor.MiddleCenter;
            huntEmptyText.color = new Color(0.75f, 0.8f, 0.82f);
            huntEmptyText.text = "Henüz av yok";
            huntEmptyText.raycastTarget = false;

            var scrollGo = CreateUiObject("hunt_scroll", contentGo.transform);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            StretchFull(scrollRect);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewportGo = CreateUiObject("hunt_viewport", scrollGo.transform);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            StretchFull(viewportRect);
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var listGo = CreateUiObject("hunt_list", viewportGo.transform);
            huntListRoot = listGo.GetComponent<RectTransform>();
            huntListRoot.anchorMin = new Vector2(0f, 1f);
            huntListRoot.anchorMax = new Vector2(1f, 1f);
            huntListRoot.pivot = new Vector2(0.5f, 1f);
            huntListRoot.anchoredPosition = Vector2.zero;
            huntListRoot.sizeDelta = new Vector2(0f, 0f);

            var layout = listGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            var fitter = listGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.content = huntListRoot;
            scroll.viewport = viewportRect;

            emptyGo.transform.SetAsLastSibling();
        }
        else
        {
            var bodyGo = CreateUiObject("panel_body", contentGo.transform);
            StretchFull(bodyGo.GetComponent<RectTransform>());
            bodyText = bodyGo.AddComponent<Text>();
            ApplyFont(bodyText);
            bodyText.fontSize = 16;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.color = new Color(0.9f, 0.93f, 0.91f);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.raycastTarget = false;
            bodyText.text = "";
        }

        var collapsible = panelGo.AddComponent<CollapsiblePlayerPanel>();
        var persistenceKey = withHuntList
            ? CollapsiblePanelPersistence.HuntKey
            : CollapsiblePanelPersistence.InventoryKey;

        collapsible.Configure(
            panelRoot,
            toggleButton,
            toggleText,
            contentGo,
            anchorMin,
            anchorMax,
            title,
            alignLeft,
            startExpanded: false,
            panelPersistenceKey: persistenceKey);

        return (collapsible, bodyText, huntListRoot, huntEmptyText);
    }

    static (
        Button placeTrap,
        Button placeTrapWithRabbit,
        Text trapStockLabel,
        Text rabbitStockLabel,
        Text rabbitBonusLabel)
        CreateCaveTrapActionStrip(Transform parent)
    {
        var panel = CreateVisualCard(
            parent,
            "CaveTrapStrip",
            new Vector2(0.5f, 0.48f),
            new Vector2(420f, 168f),
            new Color(0.06f, 0.09f, 0.1f, 0.88f));

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var trapRow = CreateCaveTrapRow(panel.transform, "Tuzak Kur", "btn_place_trap", false);
        var baitRow = CreateCaveTrapRow(
            panel.transform,
            "Tavşanlı Tuzak Kur",
            "btn_place_trap_rabbit",
            true,
            "+ ejder şansı");

        return (trapRow.button, baitRow.button, trapRow.stockLabel, baitRow.stockLabel, baitRow.bonusLabel);
    }

    static (Button button, Text stockLabel, Text bonusLabel) CreateCaveTrapRow(
        Transform parent,
        string label,
        string objectName,
        bool baitStyle,
        string bonusHint = null)
    {
        var rowGo = CreateUiObject("row_" + objectName, parent);
        var rowLe = rowGo.AddComponent<LayoutElement>();
        rowLe.preferredHeight = baitStyle ? 72f : 58f;
        rowLe.minHeight = 52f;

        var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.padding = new RectOffset(0, 0, 0, 0);

        var button = CreateButton(rowGo.transform, label, objectName, Vector2.zero);
        var buttonLe = button.gameObject.GetComponent<LayoutElement>();
        if (buttonLe == null)
            buttonLe = button.gameObject.AddComponent<LayoutElement>();
        buttonLe.flexibleWidth = 1f;
        buttonLe.preferredWidth = 240f;
        buttonLe.preferredHeight = baitStyle ? 52f : 46f;

        if (baitStyle)
        {
            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = new Color(0.28f, 0.22f, 0.38f, 0.98f);
        }

        var infoGo = CreateUiObject("stock_" + objectName, rowGo.transform);
        var infoLe = infoGo.AddComponent<LayoutElement>();
        infoLe.minWidth = 120f;
        infoLe.preferredWidth = 130f;

        var infoLayout = infoGo.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 2f;
        infoLayout.childAlignment = TextAnchor.MiddleLeft;
        infoLayout.childControlHeight = true;
        infoLayout.childForceExpandHeight = false;

        var stockLabelGo = CreateUiObject("stock_label", infoGo.transform);
        var stockText = stockLabelGo.AddComponent<Text>();
        ApplyFont(stockText);
        stockText.fontSize = 15;
        stockText.fontStyle = FontStyle.Bold;
        stockText.alignment = TextAnchor.MiddleLeft;
        stockText.color = new Color(0.85f, 0.9f, 0.92f);
        stockText.text = baitStyle ? "Tavşan: 0" : "Tuzak: 0";

        Text bonusLabel = null;
        if (!string.IsNullOrEmpty(bonusHint))
        {
            var bonusGo = CreateUiObject("bonus_label", infoGo.transform);
            bonusLabel = bonusGo.AddComponent<Text>();
            ApplyFont(bonusLabel);
            bonusLabel.fontSize = 13;
            bonusLabel.fontStyle = FontStyle.Italic;
            bonusLabel.alignment = TextAnchor.MiddleLeft;
            bonusLabel.color = new Color(0.75f, 0.65f, 0.95f);
            bonusLabel.text = bonusHint;
        }

        return (button, stockText, bonusLabel);
    }

    static void AttachCaveTrapResources(
        GameObject host,
        Button trap,
        Button baitTrap,
        Text trapStock,
        Text rabbitStock,
        Text rabbitBonus)
    {
        var presenter = host.GetComponent<CaveTrapResourcesPresenter>();
        if (presenter == null)
            presenter = host.AddComponent<CaveTrapResourcesPresenter>();
        presenter.Configure(trap, baitTrap, trapStock, rabbitStock, rabbitBonus);
    }

    static (Text day, Text time, Text gold) CreateHud(Transform parent)
    {
        var root = CreateUiObject("HUD", parent);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 62f);

        var image = root.AddComponent<Image>();
        image.color = new Color(0.04f, 0.06f, 0.07f, 0.82f);

        var layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 10, 10);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        return (
            CreateHudLabel(root.transform, "txt_day", "Gün 1"),
            CreateHudLabel(root.transform, "txt_time", "04:00"),
            CreateHudLabel(root.transform, "txt_gold", "100 altın"));
    }

    static Text CreateHudLabel(Transform parent, string objectName, string defaultText)
    {
        var go = CreateUiObject(objectName, parent);
        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 22;
        text.color = new Color(0.94f, 0.95f, 0.91f);
        text.alignment = TextAnchor.MiddleLeft;
        text.text = defaultText;
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 40f;
        return text;
    }

    static void CreateTitle(Transform parent, string title)
    {
        var go = CreateUiObject("txt_title", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.805f);
        rect.anchorMax = new Vector2(0.5f, 0.805f);
        rect.sizeDelta = new Vector2(740f, 54f);

        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 34;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.84f, 0.42f);
        text.text = title;
    }

    static void CreateSubtitle(Transform parent, string subtitle, Vector2 anchor)
    {
        var go = CreateUiObject("txt_subtitle", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(520f, 32f);

        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 17;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.82f, 0.88f, 0.84f);
        text.text = subtitle;
    }

    static Text CreateStatusText(Transform parent)
    {
        var panel = CreateVisualCard(parent, "status_panel", new Vector2(0.5f, 0.04f), new Vector2(820f, 68f), new Color(0.04f, 0.06f, 0.07f, 0.9f));
        var labelGo = CreateUiObject("status_label", panel.transform);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(8f, -4f);
        labelRect.sizeDelta = new Vector2(120f, 22f);
        var label = labelGo.AddComponent<Text>();
        ApplyFont(label);
        label.fontSize = 14;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(0.75f, 0.82f, 0.86f);
        label.text = "Durum";
        label.raycastTarget = false;

        var go = CreateUiObject("txt_status", panel.transform);
        var bodyRect = go.GetComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(12f, 8f);
        bodyRect.offsetMax = new Vector2(-12f, -22f);

        var text = go.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 16;
        text.alignment = TextAnchor.UpperLeft;
        text.color = new Color(0.88f, 0.92f, 0.94f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.05f;
        text.supportRichText = false;
        text.text = "Hazır.";
        return text;
    }

    static Button[] CreateVerticalButtonPanel(
        Transform parent,
        (string label, string objectName)[] entries,
        Vector2 anchor,
        Vector2 size)
    {
        var panel = CreateVisualCard(parent, "ActionPanel", anchor, size, new Color(0.05f, 0.08f, 0.09f, 0.78f));
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(12, 12, 12, 12);

        var buttons = new Button[entries.Length];
        for (var i = 0; i < entries.Length; i++)
        {
            buttons[i] = CreateButton(panel.transform, entries[i].label, entries[i].objectName, Vector2.zero);
            var le = buttons[i].gameObject.AddComponent<LayoutElement>();
            le.minHeight = 46f;
            le.preferredHeight = 46f;
        }

        return buttons;
    }

    static Button CreateButton(Transform parent, string label, string objectName, Vector2 anchoredPosition)
    {
        var go = CreateUiObject(objectName, parent);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.20f, 0.33f, 0.34f, 0.96f);

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
        outline.effectDistance = new Vector2(2f, -2f);

        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.20f, 0.33f, 0.34f, 0.96f);
        colors.highlightedColor = new Color(0.31f, 0.49f, 0.48f, 1f);
        colors.pressedColor = new Color(0.12f, 0.22f, 0.23f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        if (anchoredPosition != Vector2.zero)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(340f, 52f);
        }

        var textGo = CreateUiObject("Text", go.transform);
        StretchFull(textGo.GetComponent<RectTransform>());
        var text = textGo.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 20;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.97f, 0.98f, 0.94f);
        text.text = label;

        return button;
    }

    static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static GameObject CreateUiObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void ApplyFont(Text text)
    {
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}



