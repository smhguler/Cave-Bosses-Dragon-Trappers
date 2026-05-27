using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mağara sahnesi — tuzak, tavşanlı tuzak, güvenli oda, derinlik, gün bitir ve ana merkeze dönüş.
/// </summary>
public class CaveUIController : MonoBehaviour
{
    [SerializeField] Button placeTrapButton;
    [SerializeField] Button placeTrapWithRabbitButton;
    [SerializeField] Button buildSafeRoomButton;
    [SerializeField] Button travelDeeperButton;
    [SerializeField] Button endDayButton;
    [SerializeField] Button backButton;
    [SerializeField] Text statusText;

    public void Configure(
        Button trap,
        Button trapWithRabbit,
        Button safeRoom,
        Button travelDeeper,
        Button endDay,
        Button back,
        Text status)
    {
        placeTrapButton = trap;
        placeTrapWithRabbitButton = trapWithRabbit;
        buildSafeRoomButton = safeRoom;
        travelDeeperButton = travelDeeper;
        endDayButton = endDay;
        backButton = back;
        statusText = status;
        RebindButtons();
    }

    void OnEnable()
    {
        UiFeedbackLog.OnMessage += HandleMessage;
        if (AreButtonsAssigned())
            BindButtons();
        HandleMessage("Mağara — tuzak ve gün sonu.");
    }

    void OnDisable()
    {
        UiFeedbackLog.OnMessage -= HandleMessage;
        UnbindButtons();
    }

    void RebindButtons()
    {
        UnbindButtons();
        BindButtons();
    }

    bool AreButtonsAssigned() =>
        placeTrapButton != null
        && placeTrapWithRabbitButton != null
        && buildSafeRoomButton != null
        && travelDeeperButton != null
        && endDayButton != null
        && backButton != null;

    void BindButtons()
    {
        Bind(placeTrapButton, OnPlaceTrap);
        Bind(placeTrapWithRabbitButton, OnPlaceTrapWithRabbit);
        Bind(buildSafeRoomButton, OnBuildSafeRoom);
        Bind(travelDeeperButton, OnTravelDeeper);
        Bind(endDayButton, OnEndDay);
        Bind(backButton, OnBack);
    }

    void UnbindButtons()
    {
        Unbind(placeTrapButton, OnPlaceTrap);
        Unbind(placeTrapWithRabbitButton, OnPlaceTrapWithRabbit);
        Unbind(buildSafeRoomButton, OnBuildSafeRoom);
        Unbind(travelDeeperButton, OnTravelDeeper);
        Unbind(endDayButton, OnEndDay);
        Unbind(backButton, OnBack);
    }

    static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    static void Unbind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveListener(action);
    }

    void OnPlaceTrap() => GameSession.Instance?.Actions.PlaceTrap();
    void OnPlaceTrapWithRabbit() => GameSession.Instance?.Actions.PlaceTrapWithRabbit();
    void OnBuildSafeRoom() => GameSession.Instance?.Actions.BuildSafeRoom();
    void OnTravelDeeper() => GameSession.Instance?.Actions.TravelDeeper();
    void OnEndDay() => GameSession.Instance?.Actions.EndDay();
    void OnBack() => GameSession.Instance?.Actions.ReturnToHeadquarters();

    void HandleMessage(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
