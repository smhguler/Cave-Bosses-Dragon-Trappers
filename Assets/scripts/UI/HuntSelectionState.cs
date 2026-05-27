using System;

/// <summary>
/// Market satışında Avladıklarım'dan seçilen ejderha türü (UI-only).
/// </summary>
public static class HuntSelectionState
{
    public static string SelectedDragonName { get; private set; }

    public static event Action<string> SelectionChanged;

    public static void Select(string dragonName)
    {
        SelectedDragonName = dragonName;
        SelectionChanged?.Invoke(SelectedDragonName);
    }

    public static void Clear()
    {
        if (string.IsNullOrEmpty(SelectedDragonName))
            return;

        Select(null);
    }

    public static void PruneIfNotSellable(Inventory inventory, DragonType[] dragonTypes)
    {
        if (string.IsNullOrEmpty(SelectedDragonName))
            return;

        var rows = InventoryHuntCatalog.CollectHuntRows(inventory, dragonTypes);
        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i].dragonName != SelectedDragonName)
                continue;

            if (rows[i].alive > 0 || rows[i].dead > 0)
                return;
        }

        Clear();
    }
}
