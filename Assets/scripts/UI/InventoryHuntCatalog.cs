using System.Collections.Generic;

/// <summary>
/// UI-only: envanterden av / satış satırlarını toplar (inv.Dragons + toplam fallback).
/// </summary>
public static class InventoryHuntCatalog
{
    public struct HuntRow
    {
        public string dragonName;
        public int alive;
        public int dead;
    }

    public static List<HuntRow> CollectHuntRows(Inventory inventory, DragonType[] dragonTypes)
    {
        var rows = new List<HuntRow>();
        if (inventory == null)
            return rows;

        var seen = new HashSet<string>();

        var dragons = inventory.Dragons;
        for (var i = 0; i < dragons.Count; i++)
        {
            var entry = dragons[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.dragonName))
                continue;

            if (entry.alive <= 0 && entry.dead <= 0)
                continue;

            rows.Add(new HuntRow
            {
                dragonName = entry.dragonName,
                alive = entry.alive,
                dead = entry.dead,
            });
            seen.Add(entry.dragonName);
        }

        if (rows.Count == 0 && dragonTypes != null)
        {
            for (var i = 0; i < dragonTypes.Length; i++)
            {
                var type = dragonTypes[i];
                if (type == null || string.IsNullOrWhiteSpace(type.name) || seen.Contains(type.name))
                    continue;

                var alive = inventory.GetDragonCount(type.name, true);
                var dead = inventory.GetDragonCount(type.name, false);
                if (alive <= 0 && dead <= 0)
                    continue;

                rows.Add(new HuntRow
                {
                    dragonName = type.name,
                    alive = alive,
                    dead = dead,
                });
                seen.Add(type.name);
            }
        }

        return rows;
    }

    public static bool HasAnyHuntStock(Inventory inventory)
    {
        if (inventory == null)
            return false;

        if (inventory.dragonsAlive > 0 || inventory.dragonsDead > 0)
            return true;

        return CollectHuntRows(inventory, null).Count > 0;
    }
}
