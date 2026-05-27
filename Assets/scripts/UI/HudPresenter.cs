using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays day, remaining time, and gold from the active GameSession.
/// </summary>
public class HudPresenter : MonoBehaviour
{
    [SerializeField] Text dayText;
    [SerializeField] Text timeText;
    [SerializeField] Text goldText;

    public void Configure(Text day, Text time, Text gold)
    {
        dayText = day;
        timeText = time;
        goldText = gold;
    }

    void Update()
    {
        var session = GameSession.Instance;
        if (session == null)
            return;

        if (dayText != null)
            dayText.text = $"Gün {session.Day}";

        if (timeText != null)
            timeText.text = FormatClock(session.TimeLeft);

        if (goldText != null)
            goldText.text = $"{session.Gold} altın";
    }

    public static string FormatClock(float secondsLeft)
    {
        var total = Mathf.Max(0, Mathf.CeilToInt(secondsLeft));
        var minutes = total / 60;
        var seconds = total % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
