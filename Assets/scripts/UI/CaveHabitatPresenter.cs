using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mağara ekranında aktif habitat / kat adı + yakalama ilerlemesini gösterir.
/// </summary>
public class CaveHabitatPresenter : MonoBehaviour
{
    [SerializeField] Text habitatFloorText;

    public void Configure(Text habitatFloor) => habitatFloorText = habitatFloor;

    void Update() => Refresh();

    void Refresh()
    {
        if (habitatFloorText == null)
            return;

        var session = GameSession.Instance;
        if (session == null || session.Game == null)
        {
            habitatFloorText.text = "Kat: —";
            return;
        }

        var dayCycle = session.Game.dayCycle;
        var habitat = dayCycle.currentHabitat;
        if (habitat == null || !dayCycle.IsInHabitat)
        {
            habitatFloorText.text = "Kat: —";
            return;
        }

        var captures = dayCycle.CurrentHabitatCaptures;
        var score = dayCycle.CurrentHabitatScore;

        var text = $"Kat: {habitat.displayName}\nYakalama: {captures}/{habitat.requiredCaptures}";
        if (habitat.requiredScore > 0)
            text += $"\nSkor hedefi: {score}/{habitat.requiredScore}";

        habitatFloorText.text = text;
    }
}
