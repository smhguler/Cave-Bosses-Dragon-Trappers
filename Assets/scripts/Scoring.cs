[System.Serializable]
public class Scoring
{
    public int aliveDragonPoints = 2;
    public int deadDragonPoints = 1;

    public int currentScore;

    public int Recalculate(Inventory inventory, DayCycle dayCycle)
    {
        currentScore = inventory.dragonsAlive * aliveDragonPoints + inventory.dragonsDead * deadDragonPoints;
        return currentScore;
    }
}