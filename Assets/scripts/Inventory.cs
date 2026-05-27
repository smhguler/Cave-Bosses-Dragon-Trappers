using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DragonInventoryEntry
{
    public string dragonName;
    public int alive;
    public int dead;
}

[System.Serializable]
public class Inventory
{
    [Header("Starting Values")]
    public int startingGold = 100;
    public int startingTraps = 2;
    public int startingRabbits;

    [Header("Wallet")]
    public int gold = 100;

    [Header("Tools")]
    public int traps = 2;
    public int rabbits;

    [Header("Dragon Totals")]
    public int dragonsAlive;
    public int dragonsDead;

    [SerializeField] private List<DragonInventoryEntry> dragons = new List<DragonInventoryEntry>();

    public IReadOnlyList<DragonInventoryEntry> Dragons
    {
        get { return dragons; }
    }

    public void ResetForNewRun(DragonType[] dragonTypes)
    {
        gold = startingGold;
        traps = startingTraps;
        rabbits = startingRabbits;
        dragonsAlive = 0;
        dragonsDead = 0;
        dragons.Clear();
        EnsureDragonTypes(dragonTypes);
    }

    public void EnsureDragonTypes(DragonType[] dragonTypes)
    {
        if (dragons == null)
        {
            dragons = new List<DragonInventoryEntry>();
        }

        if (dragonTypes == null)
        {
            return;
        }

        for (int i = 0; i < dragonTypes.Length; i++)
        {
            DragonType dragonType = dragonTypes[i];
            if (dragonType == null || string.IsNullOrEmpty(dragonType.name))
            {
                continue;
            }

            FindOrCreateEntry(dragonType.name);
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
        {
            return false;
        }

        gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        gold += Mathf.Max(0, amount);
    }

    public bool ConsumeTrap(int amount)
    {
        if (amount < 0 || traps < amount)
        {
            return false;
        }

        traps -= amount;
        return true;
    }

    public void AddTraps(int amount)
    {
        traps += Mathf.Max(0, amount);
    }

    public bool ConsumeRabbit(int amount)
    {
        if (amount < 0 || rabbits < amount)
        {
            return false;
        }

        rabbits -= amount;
        return true;
    }

    public void AddRabbits(int amount)
    {
        rabbits += Mathf.Max(0, amount);
    }

    public void AddDragon(string dragonName, bool alive)
    {
        DragonInventoryEntry entry = FindOrCreateEntry(dragonName);

        if (alive)
        {
            entry.alive++;
            dragonsAlive++;
        }
        else
        {
            entry.dead++;
            dragonsDead++;
        }
    }

    public bool RemoveDragon(string dragonName, bool alive)
    {
        DragonInventoryEntry entry = FindEntry(dragonName);
        if (entry == null)
        {
            return false;
        }

        if (alive)
        {
            if (entry.alive <= 0)
            {
                return false;
            }

            entry.alive--;
            dragonsAlive--;
            return true;
        }

        if (entry.dead <= 0)
        {
            return false;
        }

        entry.dead--;
        dragonsDead--;
        return true;
    }

    public int GetDragonCount(string dragonName, bool alive)
    {
        DragonInventoryEntry entry = FindEntry(dragonName);
        if (entry == null)
        {
            return 0;
        }

        return alive ? entry.alive : entry.dead;
    }

    public string FindFirstDragonName(bool alive)
    {
        for (int i = 0; i < dragons.Count; i++)
        {
            DragonInventoryEntry entry = dragons[i];
            int count = alive ? entry.alive : entry.dead;
            if (count > 0)
            {
                return entry.dragonName;
            }
        }

        return null;
    }

    private DragonInventoryEntry FindEntry(string dragonName)
    {
        if (string.IsNullOrEmpty(dragonName))
        {
            return null;
        }

        for (int i = 0; i < dragons.Count; i++)
        {
            if (dragons[i].dragonName == dragonName)
            {
                return dragons[i];
            }
        }

        return null;
    }

    private DragonInventoryEntry FindOrCreateEntry(string dragonName)
    {
        DragonInventoryEntry entry = FindEntry(dragonName);
        if (entry != null)
        {
            return entry;
        }

        entry = new DragonInventoryEntry
        {
            dragonName = dragonName,
            alive = 0,
            dead = 0
        };
        dragons.Add(entry);
        return entry;
    }
}