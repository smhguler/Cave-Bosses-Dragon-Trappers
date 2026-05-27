using System.Collections.Generic;

[System.Serializable]
public class HabitatProgress
{
    public int habitatIndex;
    public int captures;
    public int score;
    public bool cleared;
}

[System.Serializable]
public class DayCycle
{
    public int day = 1;
    public GameLocation location = GameLocation.Market;
    public int currentHabitatIndex = -1;
    public HabitatModel currentHabitat;
    public bool hasSafeRoom;
    public List<HabitatProgress> habitatProgress = new List<HabitatProgress>();

    public bool IsInMarket
    {
        get { return location == GameLocation.Market; }
    }

    public bool IsInHabitat
    {
        get { return location == GameLocation.Habitat && currentHabitat != null; }
    }

    public int CurrentHabitatCaptures
    {
        get { return IsInHabitat ? GetOrCreateProgress(currentHabitatIndex).captures : 0; }
    }

    public int CurrentHabitatScore
    {
        get { return IsInHabitat ? GetOrCreateProgress(currentHabitatIndex).score : 0; }
    }

    public void ResetRun()
    {
        day = 1;
        location = GameLocation.Market;
        currentHabitatIndex = -1;
        currentHabitat = null;
        hasSafeRoom = false;

        if (habitatProgress == null)
        {
            habitatProgress = new List<HabitatProgress>();
        }

        habitatProgress.Clear();
    }

    public void EnterHabitat(HabitatModel habitat, int habitatIndex)
    {
        location = GameLocation.Habitat;
        currentHabitat = habitat;
        currentHabitatIndex = habitatIndex;
        hasSafeRoom = false;
        GetOrCreateProgress(habitatIndex);
    }

    public void GoToMarket()
    {
        location = GameLocation.Market;
        currentHabitat = null;
        currentHabitatIndex = -1;
        hasSafeRoom = false;
    }

    public void BuildSafeRoom()
    {
        hasSafeRoom = true;
    }

    public void StartNewDay(GameLocation nextLocation, HabitatModel habitat, int habitatIndex)
    {
        day++;
        location = nextLocation;
        currentHabitat = nextLocation == GameLocation.Habitat ? habitat : null;
        currentHabitatIndex = nextLocation == GameLocation.Habitat ? habitatIndex : -1;
        hasSafeRoom = false;

        if (nextLocation == GameLocation.Habitat)
        {
            GetOrCreateProgress(habitatIndex);
        }
    }

    public void RecordHabitatCapture(int scoreValue)
    {
        if (!IsInHabitat)
        {
            return;
        }

        HabitatProgress progress = GetOrCreateProgress(currentHabitatIndex);
        progress.captures++;
        progress.score += scoreValue;
    }

    public bool IsCurrentHabitatCleared()
    {
        if (!IsInHabitat || currentHabitat == null)
        {
            return false;
        }

        HabitatProgress progress = GetOrCreateProgress(currentHabitatIndex);
        return progress.cleared || currentHabitat.IsClearedBy(progress.captures, progress.score);
    }

    public bool MarkCurrentHabitatCleared()
    {
        if (!IsInHabitat)
        {
            return false;
        }

        HabitatProgress progress = GetOrCreateProgress(currentHabitatIndex);
        if (progress.cleared)
        {
            return false;
        }

        progress.cleared = true;
        return true;
    }

    public bool IsHabitatCleared(int habitatIndex)
    {
        HabitatProgress progress = FindProgress(habitatIndex);
        return progress != null && progress.cleared;
    }

    public int ClearedHabitatCount()
    {
        if (habitatProgress == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < habitatProgress.Count; i++)
        {
            if (habitatProgress[i] != null && habitatProgress[i].cleared)
            {
                count++;
            }
        }

        return count;
    }

    public bool AreAllHabitatsCleared(int totalHabitats)
    {
        return totalHabitats > 0 && ClearedHabitatCount() >= totalHabitats;
    }

    private HabitatProgress GetOrCreateProgress(int habitatIndex)
    {
        if (habitatProgress == null)
        {
            habitatProgress = new List<HabitatProgress>();
        }

        HabitatProgress progress = FindProgress(habitatIndex);
        if (progress != null)
        {
            return progress;
        }

        progress = new HabitatProgress
        {
            habitatIndex = habitatIndex,
            captures = 0,
            score = 0,
            cleared = false
        };
        habitatProgress.Add(progress);
        return progress;
    }

    private HabitatProgress FindProgress(int habitatIndex)
    {
        if (habitatProgress == null)
        {
            return null;
        }

        for (int i = 0; i < habitatProgress.Count; i++)
        {
            HabitatProgress progress = habitatProgress[i];
            if (progress != null && progress.habitatIndex == habitatIndex)
            {
                return progress;
            }
        }

        return null;
    }
}
