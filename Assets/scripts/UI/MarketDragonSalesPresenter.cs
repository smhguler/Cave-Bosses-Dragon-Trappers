using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Market ejderha satış listesi — tüm satılabilir türler; Avladıklarım seçimi vurgulanır.
/// </summary>
public class MarketDragonSalesPresenter : MonoBehaviour
{
    readonly List<GameObject> rowObjects = new List<GameObject>();

    [SerializeField] RectTransform listRoot;
    [SerializeField] Text emptyLabel;

    string lastInventorySignature = "";

    public void Configure(RectTransform list, Text empty)
    {
        listRoot = list;
        emptyLabel = empty;
        Refresh();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += OnInventoryMaybeChanged;
        HuntSelectionState.SelectionChanged += OnInventoryMaybeChanged;
        Refresh();
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= OnInventoryMaybeChanged;
        HuntSelectionState.SelectionChanged -= OnInventoryMaybeChanged;
    }

    void Update()
    {
        var signature = BuildInventorySignature();
        if (signature == lastInventorySignature)
            return;

        lastInventorySignature = signature;
        Refresh();
    }

    void OnInventoryMaybeChanged(string message) => lastInventorySignature = "";

    void Refresh()
    {
        if (listRoot == null)
            return;

        ClearRows();

        var session = GameSession.Instance;
        if (session == null || session.Game == null)
        {
            SetEmpty(true, "Satılacak ejderha yok");
            return;
        }

        var game = session.Game;
        HuntSelectionState.PruneIfNotSellable(game.inventory, game.dragonTypes);

        var sellable = BuildSellableRows(game);
        if (sellable.Count == 0)
        {
            HuntSelectionState.Clear();
            SetEmpty(true, "Satılacak ejderha yok");
            return;
        }

        SetEmpty(false, null);
        var selected = HuntSelectionState.SelectedDragonName;
        for (var i = 0; i < sellable.Count; i++)
            CreateRow(sellable[i], game, selected);
    }

    struct SellableDragonRow
    {
        public string name;
        public int alive;
        public int dead;
        public int alivePrice;
        public int deadPrice;
    }

    static List<SellableDragonRow> BuildSellableRows(PrototypeGameLoop game)
    {
        var list = new List<SellableDragonRow>();
        var huntRows = InventoryHuntCatalog.CollectHuntRows(game.inventory, game.dragonTypes);

        for (var i = 0; i < huntRows.Count; i++)
        {
            var row = huntRows[i];
            var prices = DragonTypePriceLookup.Resolve(game.dragonTypes, row.dragonName);
            list.Add(new SellableDragonRow
            {
                name = row.dragonName,
                alive = row.alive,
                dead = row.dead,
                alivePrice = prices.alivePrice,
                deadPrice = prices.deadPrice,
            });
        }

        return list;
    }

    void CreateRow(SellableDragonRow row, PrototypeGameLoop game, string selectedName)
    {
        var isSelected = !string.IsNullOrEmpty(selectedName)
            && row.name == selectedName;

        var rowGo = new GameObject("dragon_row_" + row.name, typeof(RectTransform));
        rowGo.transform.SetParent(listRoot, false);
        rowObjects.Add(rowGo);

        var rowRect = rowGo.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 108f);

        var rowImage = rowGo.AddComponent<Image>();
        rowImage.color = isSelected
            ? new Color(0.14f, 0.22f, 0.2f, 0.98f)
            : new Color(0.08f, 0.11f, 0.12f, 0.92f);

        var layout = rowGo.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = false;

        var infoGo = new GameObject("info", typeof(RectTransform));
        infoGo.transform.SetParent(rowGo.transform, false);
        var infoLe = infoGo.AddComponent<LayoutElement>();
        infoLe.flexibleWidth = 1f;
        infoLe.minWidth = 220f;
        infoLe.preferredHeight = 92f;

        var infoText = infoGo.AddComponent<Text>();
        ApplyFont(infoText);
        infoText.fontSize = 15;
        infoText.alignment = TextAnchor.MiddleLeft;
        infoText.color = new Color(0.9f, 0.93f, 0.91f);
        infoText.horizontalOverflow = HorizontalWrapMode.Wrap;
        infoText.verticalOverflow = VerticalWrapMode.Overflow;
        infoText.text =
            $"{row.name}\n" +
            $"Canlı: {row.alive} — satış {row.alivePrice} altın\n" +
            $"Ölü: {row.dead} — satış {row.deadPrice} altın";

        var sellAlive = CreateRowButton(
            rowGo.transform,
            "btn_sell_alive_" + row.name,
            "Canlı Sat (-2 puan)",
            row.alive > 0);
        sellAlive.onClick.AddListener(() => OnSell(row.name, true));

        var sellDead = CreateRowButton(
            rowGo.transform,
            "btn_sell_dead_" + row.name,
            "Ölü Sat (-1 puan)",
            row.dead > 0);
        sellDead.onClick.AddListener(() => OnSell(row.name, false));
    }

    static Button CreateRowButton(Transform parent, string objectName, string label, bool enabled)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 100f;
        le.preferredWidth = 108f;
        le.preferredHeight = 40f;

        var image = go.AddComponent<Image>();
        image.color = enabled
            ? new Color(0.22f, 0.36f, 0.38f, 0.96f)
            : new Color(0.15f, 0.15f, 0.15f, 0.6f);

        var button = go.AddComponent<Button>();
        button.interactable = enabled;
        var colors = button.colors;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        button.colors = colors;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 15;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;

        return button;
    }

    static void OnSell(string dragonName, bool alive)
    {
        var session = GameSession.Instance;
        if (session == null)
            return;

        session.Actions.SellDragon(dragonName, alive);

        var game = session.Game;
        if (game != null)
            HuntSelectionState.PruneIfNotSellable(game.inventory, game.dragonTypes);
    }

    static string BuildInventorySignature()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return "";

        var inv = session.Game.inventory;
        var sb = new System.Text.StringBuilder();
        sb.Append(inv.gold).Append('|').Append(inv.dragonsAlive).Append('|').Append(inv.dragonsDead);
        sb.Append('|').Append(HuntSelectionState.SelectedDragonName);

        var rows = InventoryHuntCatalog.CollectHuntRows(inv, session.Game.dragonTypes);
        for (var i = 0; i < rows.Count; i++)
            sb.Append(rows[i].dragonName).Append(':').Append(rows[i].alive).Append('/').Append(rows[i].dead).Append(';');

        return sb.ToString();
    }

    void SetEmpty(bool show, string message)
    {
        if (emptyLabel != null)
        {
            emptyLabel.gameObject.SetActive(show);
            if (show && !string.IsNullOrEmpty(message))
                emptyLabel.text = message;
        }

        listRoot.gameObject.SetActive(!show);
    }

    void ClearRows()
    {
        for (var i = 0; i < rowObjects.Count; i++)
        {
            if (rowObjects[i] == null)
                continue;

            if (Application.isPlaying)
                Destroy(rowObjects[i]);
            else
                DestroyImmediate(rowObjects[i]);
        }

        rowObjects.Clear();
    }

    static void ApplyFont(Text text)
    {
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
