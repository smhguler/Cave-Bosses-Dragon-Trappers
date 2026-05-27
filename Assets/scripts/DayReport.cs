[System.Serializable]
public class DayReport
{
    public int dayNumber;
    public int startingGold;
    public int endingGold;
    public int endingScore;
    public int totalIncome;
    public int totalExpense;
    public int netProfit;
    public int trapsBought;
    public int rabbitsBought;
    public int liveDragonsSold;
    public int deadDragonsSold;
    public int liveDragonsCaptured;
    public int deadDragonsCaptured;
    public int normalTrapsPlaced;
    public int rabbitBaitTrapsPlaced;
    public int safeRoomsBuilt;
    public int safeRoomTrapsSpent;
    public bool runResetAtDayEnd;
    public bool habitatCleared;
    public bool allCavesCompleted;
    public bool finalized;

    public void Begin(int day, int gold)
    {
        dayNumber = day;
        startingGold = gold;
        endingGold = gold;
        endingScore = 0;
        totalIncome = 0;
        totalExpense = 0;
        netProfit = 0;
        trapsBought = 0;
        rabbitsBought = 0;
        liveDragonsSold = 0;
        deadDragonsSold = 0;
        liveDragonsCaptured = 0;
        deadDragonsCaptured = 0;
        normalTrapsPlaced = 0;
        rabbitBaitTrapsPlaced = 0;
        safeRoomsBuilt = 0;
        safeRoomTrapsSpent = 0;
        runResetAtDayEnd = false;
        habitatCleared = false;
        allCavesCompleted = false;
        finalized = false;
    }

    public void AddIncome(int amount)
    {
        totalIncome += System.Math.Max(0, amount);
        Recalculate();
    }

    public void AddExpense(int amount)
    {
        totalExpense += System.Math.Max(0, amount);
        Recalculate();
    }

    public void Complete(int gold, int score, bool runReset, bool habitatWasCleared, bool allCavesWereCompleted)
    {
        endingGold = gold;
        endingScore = score;
        runResetAtDayEnd = runReset;
        habitatCleared = habitatCleared || habitatWasCleared;
        allCavesCompleted = allCavesCompleted || allCavesWereCompleted;
        finalized = true;
        Recalculate();
    }

    public void Recalculate()
    {
        netProfit = totalIncome - totalExpense;
    }
}
