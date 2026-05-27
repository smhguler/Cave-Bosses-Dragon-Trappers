using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gün sonu raporunu modal panelde gösterir ve rapor hazır olduğunda kısa bilgi mesajı yayınlar.
/// </summary>
public class DayReportPanelPresenter : MonoBehaviour
{
    [SerializeField] Button openReportButton;
    [SerializeField] Text openReportButtonLabel;
    [SerializeField] GameObject panelRoot;
    [SerializeField] Text reportBodyText;
    [SerializeField] Button closeButton;

    DayReport notifiedReport;
    DayReport lastRenderedReport;

    public void Configure(
        Button openButton,
        Text openLabel,
        GameObject panel,
        Text body,
        Button close)
    {
        openReportButton = openButton;
        openReportButtonLabel = openLabel;
        panelRoot = panel;
        reportBodyText = body;
        closeButton = close;
        Rebind();
        Refresh();
    }

    void OnEnable()
    {
        Rebind();
        Refresh();
    }

    void OnDisable() => Unbind();

    void Update() => Refresh();

    void Rebind()
    {
        Unbind();

        if (openReportButton != null)
            openReportButton.onClick.AddListener(OnOpenReport);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseReport);
    }

    void Unbind()
    {
        if (openReportButton != null)
            openReportButton.onClick.RemoveListener(OnOpenReport);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseReport);
    }

    void Refresh()
    {
        var report = GetLatestReport();
        var hasReport = report != null;

        if (openReportButton != null)
            openReportButton.interactable = hasReport;

        if (openReportButtonLabel != null)
            openReportButtonLabel.text = hasReport ? "Rapor" : "Rapor (yok)";

        if (panelRoot != null && panelRoot.activeSelf && !hasReport)
            panelRoot.SetActive(false);

        if (hasReport && !ReferenceEquals(report, notifiedReport))
        {
            notifiedReport = report;
            UiFeedbackLog.Publish("Gün raporu hazır.");
            ShowReport(report);
        }
        else if (hasReport && panelRoot != null && panelRoot.activeSelf && !ReferenceEquals(report, lastRenderedReport))
        {
            ShowReport(report);
        }
    }

    void OnOpenReport()
    {
        var report = GetLatestReport();
        if (report == null)
        {
            UiFeedbackLog.Publish("Henüz rapor yok.");
            return;
        }

        ShowReport(report);
    }

    void OnCloseReport()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    DayReport GetLatestReport()
    {
        var session = GameSession.Instance;
        if (session == null || session.Game == null)
            return null;

        return session.Game.GetLastDayReport() ?? session.Game.lastCompletedDayReport;
    }

    void ShowReport(DayReport report)
    {
        if (report == null)
            return;

        if (reportBodyText != null)
            reportBodyText.text = BuildReportText(report);

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        lastRenderedReport = report;
    }

    static string BuildReportText(DayReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Gün: {report.dayNumber}");
        sb.AppendLine($"Başlangıç altını: {report.startingGold}");
        sb.AppendLine($"Gün sonu altını: {report.endingGold}");
        sb.AppendLine($"Gelir: {report.totalIncome}");
        sb.AppendLine($"Gider: {report.totalExpense}");
        sb.AppendLine($"Net kazanç: {FormatNetProfit(report.netProfit)}");
        sb.AppendLine($"Gün sonu skoru: {report.endingScore}");

        AppendIfPositive(sb, "Alınan tuzak", report.trapsBought);
        AppendIfPositive(sb, "Alınan tavşan", report.rabbitsBought);
        AppendIfPositive(sb, "Kurulan normal tuzak", report.normalTrapsPlaced);
        AppendIfPositive(sb, "Kurulan tavşanlı tuzak", report.rabbitBaitTrapsPlaced);
        AppendIfPositive(sb, "Yakalanan canlı ejderha", report.liveDragonsCaptured);
        AppendIfPositive(sb, "Yakalanan ölü ejderha", report.deadDragonsCaptured);
        AppendIfPositive(sb, "Satılan canlı ejderha", report.liveDragonsSold);
        AppendIfPositive(sb, "Satılan ölü ejderha", report.deadDragonsSold);
        AppendIfPositive(sb, "Kurulan güvenli oda", report.safeRoomsBuilt);
        AppendIfPositive(sb, "Güvenli oda tuzak maliyeti", report.safeRoomTrapsSpent);

        sb.AppendLine($"Habitat temizlendi mi: {ToYesNo(report.habitatCleared)}");
        sb.AppendLine($"Tüm mağaralar tamamlandı mı: {ToYesNo(report.allCavesCompleted)}");
        sb.AppendLine($"Run reset oldu mu: {ToYesNo(report.runResetAtDayEnd)}");
        return sb.ToString();
    }

    static void AppendIfPositive(StringBuilder sb, string label, int value)
    {
        if (value <= 0)
            return;

        sb.AppendLine($"{label}: {value}");
    }

    static string FormatNetProfit(int netProfit)
    {
        if (netProfit > 0)
            return $"<color=#4CFF6A><b>{netProfit}</b></color>";

        if (netProfit < 0)
            return $"<color=#FF5A5A><b>{netProfit}</b></color>";

        return $"<color=#D0D6DC><b>{netProfit}</b></color>";
    }

    static string ToYesNo(bool value) => value ? "Evet" : "Hayır";
}
