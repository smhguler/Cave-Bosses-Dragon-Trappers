using System;
using UnityEngine;

/// <summary>
/// Gün, zaman ve gece-gündüz döngüsünü yöneten sistem.
/// Bu sınıf Serializable olduğu için PrototypeGame içinde inspector'dan ayarlanabilir.
/// </summary>
[System.Serializable]
public class TimeSystem
{
    [Header("Time")]
    [Tooltip("Oyunun başladığı gün")]
    public int day = 1;

    [Tooltip("Bir günün saniye cinsinden süresi")]
    public float dayDuration = 240f;

    [Tooltip("Gün için kalan süre")]
    public float timeLeft = 0f;

    [Tooltip("Mağarada güvenli oda kurulu mu?")]
    public bool hasSafeRoom = false;

    /// <summary>
    /// Yeni bir koşu için zaman değerlerini hazırlar.
    /// </summary>
    public void InitializeForNewRun()
    {
        // Gün başlangıcında kalan süreyi tam gün süresine eşitliyoruz.
        timeLeft = dayDuration;
    }

    /// <summary>
    /// Verilen süreyi harcamaya çalışır.
    /// Başarılı olursa true, yetmezse false döner.
    /// Log metni ve süre bitişi için callback kullanır.
    /// </summary>
    public bool SpendTime(float seconds, string reason, Action<string> logCallback, Action onTimeOver)
    {
        if (timeLeft < seconds)
        {
            logCallback?.Invoke($"{reason} için süre yetmiyor! Gerekli: {seconds:0}s | Kalan: {timeLeft:0}s");
            return false;
        }

        timeLeft -= seconds;
        logCallback?.Invoke($"{reason} (-{seconds:0}s) | Kalan: {timeLeft:0}s");

        if (timeLeft <= 0f)
        {
            onTimeOver?.Invoke();
            return false;
        }

        return true;
    }
}

