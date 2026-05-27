[System.Serializable]
public class GameActionResult
{
    public bool success;
    public string message;
    public bool dayAdvanced;
    public bool runReset;
    public bool habitatCleared;
    public bool allCavesCompleted;
    public RunTargetState runState;
    public int day;
    public int score;

    public static GameActionResult Success(string message)
    {
        return new GameActionResult
        {
            success = true,
            message = message
        };
    }

    public static GameActionResult Failure(string message)
    {
        return new GameActionResult
        {
            success = false,
            message = message
        };
    }

    public void AppendMessage(string extra)
    {
        if (string.IsNullOrEmpty(extra))
        {
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            message = extra;
            return;
        }

        message += "\n" + extra;
    }
}
