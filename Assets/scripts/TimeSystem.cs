using UnityEngine;

[System.Serializable]
public class TimeSystem
{
    [Header("Time")]
    [Tooltip("One in-game day duration in seconds.")]
    [Min(1f)] public float dayDuration = 240f;

    [Tooltip("Remaining time in the current day.")]
    [Min(0f)] public float timeLeft;

    public bool IsDayOver
    {
        get { return timeLeft <= 0f; }
    }

    public float Normalized
    {
        get
        {
            if (dayDuration <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(timeLeft / dayDuration);
        }
    }

    public void InitializeForNewRun()
    {
        ResetDay();
    }

    public void ResetDay()
    {
        timeLeft = Mathf.Max(1f, dayDuration);
    }

    public bool TrySpend(float seconds, string reason, out string message)
    {
        seconds = Mathf.Max(0f, seconds);

        if (timeLeft + Mathf.Epsilon < seconds)
        {
            message = string.Format("{0} için süre yetmiyor. Gerekli: {1:0}s | Kalan: {2:0}s", reason, seconds, timeLeft);
            return false;
        }

        timeLeft = Mathf.Max(0f, timeLeft - seconds);
        message = string.Format("{0} (-{1:0}s) | Kalan: {2:0}s", reason, seconds, timeLeft);
        return true;
    }
}