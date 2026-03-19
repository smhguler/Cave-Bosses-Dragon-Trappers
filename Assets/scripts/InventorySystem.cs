using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyuncunun tüm envanter ve yakalanan ejderha sayılarını tutan sistem.
/// Altın, tuzak, tavşan ve ejderha sayıları burada merkezileştirilir.
/// </summary>
[System.Serializable]
public class InventorySystem
{
    [Header("Core")]
    [Tooltip("Oyuncunun sahip olduğu altın miktarı")]
    public int gold = 100;

    [Header("Inventory")]
    [Tooltip("Oyuncunun elindeki tuzak sayısı")]
    public int traps = 2;

    [Tooltip("Oyuncunun elindeki tavşan yemi sayısı")]
    public int rabbits = 0;

    [Header("Dragons (Totals)")]
    [Tooltip("Canlı yakalanmış ejderha sayısı")]
    public int dragonsAlive = 0;

    [Tooltip("Ölü yakalanmış ejderha sayısı")]
    public int dragonsDead = 0;

    // Tür bazlı sayım için sözlükler.
    // Unity bu sözlükleri inspector'da göstermez, oyun içinde kod tarafından kullanılır.
    private Dictionary<string, int> aliveByType = new Dictionary<string, int>();
    private Dictionary<string, int> deadByType = new Dictionary<string, int>();

    /// <summary>
    /// Canlı ejderha adetlerine erişim için okunabilir property.
    /// </summary>
    public Dictionary<string, int> AliveByType => aliveByType;

    /// <summary>
    /// Ölü ejderha adetlerine erişim için okunabilir property.
    /// </summary>
    public Dictionary<string, int> DeadByType => deadByType;

    /// <summary>
    /// Verilen ejderha türleri için sözlükleri sıfırlayıp anahtarları hazırlar.
    /// </summary>
    public void InitializeDragonTypes(DragonType[] dragonTypes)
    {
        aliveByType.Clear();
        deadByType.Clear();

        if (dragonTypes == null)
            return;

        foreach (var dt in dragonTypes)
        {
            if (dt == null || string.IsNullOrEmpty(dt.name))
                continue;

            if (!aliveByType.ContainsKey(dt.name)) aliveByType.Add(dt.name, 0);
            if (!deadByType.ContainsKey(dt.name)) deadByType.Add(dt.name, 0);
        }
    }

    /// <summary>
    /// Yeni bir koşu için tüm envanteri ve ejderha sayılarını sıfırlar.
    /// </summary>
    public void ResetForNewRun(DragonType[] dragonTypes)
    {
        gold = 100;
        traps = 2;
        rabbits = 0;

        dragonsAlive = 0;
        dragonsDead = 0;

        InitializeDragonTypes(dragonTypes);
    }
}

