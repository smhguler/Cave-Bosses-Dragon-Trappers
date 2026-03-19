using System;
using UnityEngine;

/// <summary>
/// Satıcının stok ve dinamik fiyat hesaplamasını yöneten sistem.
/// Stok miktarlarına ve güne bağlı olarak alış/satış fiyatlarını belirler.
/// </summary>
[System.Serializable]
public class VendorSystem
{
    [Header("Vendor Stock")]
    [Tooltip("Satıcının elindeki tuzak sayısı")]
    public int vendorTraps = 5;

    [Tooltip("Satıcının elindeki tavşan sayısı")]
    public int vendorRabbits = 3;

    [Header("Vendor Targets")]
    [Tooltip("Satıcının tuzak için hedef stok seviyesi")]
    public int vendorTrapTarget = 6;

    [Tooltip("Satıcının tavşan için hedef stok seviyesi")]
    public int vendorRabbitTarget = 4;

    [Header("Vendor Base Prices")]
    [Tooltip("Tuzak için temel alış fiyatı")]
    public int trapBaseBuy = 20;

    [Tooltip("Tavşan için temel alış fiyatı")]
    public int rabbitBaseBuy = 10;

    [Header("Vendor Prices (dynamic)")]
    [Tooltip("Güncel tuzak alış fiyatı")]
    public int trapBuyPrice;

    [Tooltip("Güncel tuzak satış fiyatı")]
    public int trapSellPrice;

    [Tooltip("Güncel tavşan alış fiyatı")]
    public int rabbitBuyPrice;

    [Tooltip("Güncel tavşan satış fiyatı")]
    public int rabbitSellPrice;

    /// <summary>
    /// Yeni bir günde satıcının stok ve fiyatlarını günceller.
    /// Gün bilgisi TimeSystem'dan dışarıdan verilir.
    /// </summary>
    public void VendorNewDay(bool silentLog, int currentDay, Action<string> logCallback)
    {
        vendorTraps = Mathf.Clamp(vendorTraps + UnityEngine.Random.Range(1, 4), 0, vendorTrapTarget);
        vendorRabbits = Mathf.Clamp(vendorRabbits + UnityEngine.Random.Range(1, 3), 0, vendorRabbitTarget);

        RecalculateVendorPrices(currentDay);

        if (!silentLog && logCallback != null)
        {
            logCallback("Satıcı stoklarını yeniledi, fiyatlar güncellendi.");
        }
    }

    /// <summary>
    /// Mevcut stok ve hedef değerlerine göre dinamik fiyatları günceller.
    /// UI güncellemesini dışarıdan yapmak için sadece veriyi değiştirir.
    /// </summary>
    public void RecalculateVendorPrices(int currentDay)
    {
        trapBuyPrice = CalcBuyPrice(trapBaseBuy, vendorTraps, vendorTrapTarget, currentDay);
        rabbitBuyPrice = CalcBuyPrice(rabbitBaseBuy, vendorRabbits, vendorRabbitTarget, currentDay);

        trapSellPrice = Mathf.Max(1, Mathf.RoundToInt(trapBuyPrice * 0.6f));
        rabbitSellPrice = Mathf.Max(1, Mathf.RoundToInt(rabbitBuyPrice * 0.6f));
    }

    /// <summary>
    /// Tek bir çeşit için stok/hedef ve gün bilgisine göre alış fiyatını hesaplar.
    /// </summary>
    public int CalcBuyPrice(int basePrice, int stock, int target, int currentDay)
    {
        float ratio = (target <= 0) ? 1f : Mathf.Clamp01(stock / (float)target);
        float multiplier = Mathf.Lerp(1.6f, 0.8f, ratio);
        float inflation = 1f + (currentDay - 1) * 0.01f; // %1/gün
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * multiplier * inflation));
    }
}

