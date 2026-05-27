using System;
using System.Collections.Generic;

/// <summary>
/// UI durum satırı için olay kanalı; son mesajlar ve kritik satırlar korunur.
/// </summary>
public static class UiFeedbackLog
{
    const int MaxHistory = 5;
    const int MaxDisplayLength = 140;

    static readonly Queue<string> RecentMessages = new Queue<string>();

    public static event Action<string> OnMessage;

    public static string LastMessage { get; private set; }

    public static void Publish(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        LastMessage = message.Trim();
        EnqueueHistory(LastMessage);
        OnMessage?.Invoke(message);
    }

    public static string GetDisplayText()
    {
        if (RecentMessages.Count == 0)
            return string.IsNullOrEmpty(LastMessage) ? "" : FormatSingleLine(LastMessage);

        var critical = FindLatestCritical();
        if (!string.IsNullOrEmpty(critical))
            return FormatSingleLine(critical);

        return FormatSingleLine(LastMessage);
    }

    public static void ResetForTests()
    {
        RecentMessages.Clear();
        LastMessage = null;
    }

    static void EnqueueHistory(string message)
    {
        RecentMessages.Enqueue(message);
        while (RecentMessages.Count > MaxHistory)
            RecentMessages.Dequeue();
    }

    static string FindLatestCritical()
    {
        string found = null;
        foreach (var message in RecentMessages)
        {
            if (!ContainsCriticalKeyword(message))
                continue;

            found = message;
        }

        if (string.IsNullOrEmpty(found))
            return null;

        var lines = found.Split('\n');
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
                continue;

            if (ContainsCriticalKeyword(line))
                return line;
        }

        return found;
    }

    static bool ContainsCriticalKeyword(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.IndexOf("failure", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("basarisiz", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("yetersiz", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("yok", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("hata", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("tavsan", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("temizlendi", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("tamamlandi", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("tuzak", StringComparison.OrdinalIgnoreCase) >= 0 && text.IndexOf("yok", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string FormatSingleLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "";

        var lines = message.Split('\n');
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.Length > 0)
                return Truncate(line, MaxDisplayLength);
        }

        return Truncate(message.Trim(), MaxDisplayLength);
    }

    static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 1) + "…";
    }
}
