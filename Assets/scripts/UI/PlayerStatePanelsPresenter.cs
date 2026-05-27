using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Katlanabilir çanta ve avladıklarım panellerini GameSession verisinden günceller.
/// </summary>
public class PlayerStatePanelsPresenter : MonoBehaviour
{
    readonly List<GameObject> huntRowObjects = new List<GameObject>();

    [SerializeField] CollapsiblePlayerPanel inventoryPanel;
    [SerializeField] Text inventoryBodyText;
    [SerializeField] CollapsiblePlayerPanel huntPanel;
    [SerializeField] RectTransform huntListRoot;
    [SerializeField] Text huntEmptyText;

    string lastSignature = "";

    public bool HasHuntListConfigured => huntListRoot != null;

    public void Configure(
        CollapsiblePlayerPanel inventory,
        Text inventoryBody,
        CollapsiblePlayerPanel hunt,
        RectTransform huntList,
        Text huntEmpty)
    {
        inventoryPanel = inventory;
        inventoryBodyText = inventoryBody;
        huntPanel = hunt;
        huntListRoot = huntList;
        huntEmptyText = huntEmpty;

        if (huntPanel != null)
        {
            huntPanel.ExpandedChanged -= OnHuntPanelExpanded;
            huntPanel.ExpandedChanged += OnHuntPanelExpanded;
        }

        ApplyPersistedPanelState();
        TryResolveHuntListFromHierarchy();
        lastSignature = "";
        Refresh();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += OnStateMaybeChanged;
        HuntSelectionState.SelectionChanged += OnHuntSelectionChanged;
        ApplyPersistedPanelState();
        TryResolveHuntListFromHierarchy();
        lastSignature = "";
        Refresh();
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= OnStateMaybeChanged;
        HuntSelectionState.SelectionChanged -= OnHuntSelectionChanged;

        if (huntPanel != null)
            huntPanel.ExpandedChanged -= OnHuntPanelExpanded;
    }

    void Update()
    {
        var signature = BuildSignature();
        if (signature == lastSignature)
            return;

        lastSignature = signature;
        Refresh();
    }

    void OnStateMaybeChanged(string message)
    {
        lastSignature = "";
        Refresh();
    }

    void ApplyPersistedPanelState()
    {
        if (inventoryPanel != null
            && CollapsiblePanelPersistence.TryGetExpanded(CollapsiblePanelPersistence.InventoryKey, out var invOpen))
        {
            inventoryPanel.SetExpanded(invOpen);
        }

        if (huntPanel != null
            && CollapsiblePanelPersistence.TryGetExpanded(CollapsiblePanelPersistence.HuntKey, out var huntOpen))
        {
            huntPanel.SetExpanded(huntOpen);
        }
    }

    void OnHuntSelectionChanged(string dragonName) => lastSignature = "";

    void OnHuntPanelExpanded(bool expanded)
    {
        if (expanded)
        {
            lastSignature = "";
            Refresh();
        }
    }

    void TryResolveHuntListFromHierarchy()
    {
        if (huntListRoot != null)
            return;

        var panels = GetComponentsInChildren<CollapsiblePlayerPanel>(true);
        for (var i = 0; i < panels.Length; i++)
        {
            var panel = panels[i];
            if (panel == null || panel.gameObject.name != "HuntPanel")
                continue;

            var list = panel.transform.Find("panel_content/hunt_scroll/hunt_viewport/hunt_list")
                ?? panel.transform.Find("panel_content/hunt_list");
            if (list != null)
                huntListRoot = list.GetComponent<RectTransform>();
        }
    }

    void Refresh()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return;

        var game = session.Game;
        var inv = game.inventory;

        if (inventoryBodyText != null)
            inventoryBodyText.text = BuildInventoryBody(inv);

        RefreshHuntList(inv, game.dragonTypes, IsMarketContext());
    }

    static string BuildInventoryBody(Inventory inv)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Altın: {inv.gold}");
        sb.AppendLine($"Tuzak: {inv.traps}");
        sb.AppendLine($"Tavşan: {inv.rabbits}");
        return sb.ToString();
    }

    void RefreshHuntList(Inventory inv, DragonType[] dragonTypes, bool marketContext)
    {
        TryResolveHuntListFromHierarchy();

        if (huntListRoot == null)
            return;

        ClearHuntRows();

        var rows = InventoryHuntCatalog.CollectHuntRows(inv, dragonTypes);
        var any = rows.Count > 0 || inv.dragonsAlive > 0 || inv.dragonsDead > 0;

        for (var i = 0; i < rows.Count; i++)
            CreateHuntRow(rows[i], marketContext);

        if (rows.Count == 0 && (inv.dragonsAlive > 0 || inv.dragonsDead > 0))
        {
            CreateHuntRow(new InventoryHuntCatalog.HuntRow
            {
                dragonName = "Ejderha stoğu",
                alive = inv.dragonsAlive,
                dead = inv.dragonsDead,
            }, false);
        }

        if (huntEmptyText != null)
        {
            huntEmptyText.gameObject.SetActive(!any);
            huntEmptyText.text = marketContext
                ? "Henüz av yok.\nSatış için tür seçin."
                : "Henüz av yok";
        }

        huntListRoot.gameObject.SetActive(any);
        LayoutRebuilder.ForceRebuildLayoutImmediate(huntListRoot);
    }

    void CreateHuntRow(InventoryHuntCatalog.HuntRow row, bool selectable)
    {
        var rowGo = new GameObject("hunt_row_" + row.dragonName, typeof(RectTransform));
        rowGo.transform.SetParent(huntListRoot, false);
        huntRowObjects.Add(rowGo);

        var layoutElement = rowGo.AddComponent<LayoutElement>();
        layoutElement.minHeight = 44f;
        layoutElement.preferredHeight = selectable ? 52f : 44f;
        layoutElement.flexibleWidth = 1f;

        var selected = selectable
            && string.Equals(HuntSelectionState.SelectedDragonName, row.dragonName, System.StringComparison.Ordinal);

        Image rowImage = rowGo.AddComponent<Image>();
        rowImage.color = selected
            ? new Color(0.28f, 0.42f, 0.38f, 0.98f)
            : new Color(0.1f, 0.14f, 0.15f, 0.92f);

        if (selectable)
        {
            var button = rowGo.AddComponent<Button>();
            var dragonName = row.dragonName;
            button.onClick.AddListener(() => HuntSelectionState.Select(dragonName));
            button.targetGraphic = rowImage;
        }
        else
        {
            rowImage.raycastTarget = false;
        }

        var textGo = new GameObject("label", typeof(RectTransform));
        textGo.transform.SetParent(rowGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        StretchRect(textRect);
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        var text = textGo.AddComponent<Text>();
        ApplyFont(text);
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = new Color(0.9f, 0.93f, 0.91f);
        text.raycastTarget = false;
        text.text = $"{row.dragonName}\nCanlı {row.alive} | Ölü {row.dead}";
    }

    void ClearHuntRows()
    {
        for (var i = 0; i < huntRowObjects.Count; i++)
        {
            if (huntRowObjects[i] == null)
                continue;

            if (Application.isPlaying)
                Destroy(huntRowObjects[i]);
            else
                DestroyImmediate(huntRowObjects[i]);
        }

        huntRowObjects.Clear();
    }

    static string BuildSignature()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return "";

        var inv = session.Game.inventory;
        var sb = new StringBuilder();
        sb.Append(inv.gold).Append('|').Append(inv.traps).Append('|').Append(inv.rabbits);
        sb.Append('|').Append(inv.dragonsAlive).Append('|').Append(inv.dragonsDead);
        sb.Append('|').Append(HuntSelectionState.SelectedDragonName);
        sb.Append('|').Append(IsMarketContext());

        var dragons = inv.Dragons;
        for (var i = 0; i < dragons.Count; i++)
        {
            var e = dragons[i];
            if (e == null)
                continue;

            sb.Append(e.dragonName).Append(':').Append(e.alive).Append('/').Append(e.dead).Append(';');
        }

        return sb.ToString();
    }

    static bool IsMarketContext() => Object.FindObjectOfType<MarketUIController>() != null;

    static void StretchRect(RectTransform rect)
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
