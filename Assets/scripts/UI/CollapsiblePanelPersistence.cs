/// <summary>
/// Çanta / Avladıklarım açık-kapalı durumu sahne geçişlerinde korunur (UI-only).
/// </summary>
public static class CollapsiblePanelPersistence
{
    public const string InventoryKey = "inventory";
    public const string HuntKey = "hunt";

    public static bool TryGetExpanded(string key, out bool expanded)
    {
        expanded = false;
        if (string.IsNullOrEmpty(key))
            return false;

        if (key == InventoryKey)
        {
            expanded = inventoryExpanded;
            return true;
        }

        if (key == HuntKey)
        {
            expanded = huntExpanded;
            return true;
        }

        return false;
    }

    public static void SetExpanded(string key, bool expanded)
    {
        if (key == InventoryKey)
            inventoryExpanded = expanded;
        else if (key == HuntKey)
            huntExpanded = expanded;
    }

    static bool inventoryExpanded;
    static bool huntExpanded;
}
