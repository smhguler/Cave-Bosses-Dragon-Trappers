using UnityEngine;

/// <summary>
/// Ejderha türü verilerini tutan basit veri sınıfı.
/// Bu sınıf, inspector üzerinden düzenlenebilmesi için Serializable yapıldı.
/// </summary>
[System.Serializable]
public class DragonType
{
    [Tooltip("Ejderhanın görünen adı")]
    public string name;

    [Tooltip("Nadirlilik (1=Common, 5=Legendary)")]
    [Range(1, 5)] public int rarity;            // 1 = common, 5 = legendary

    [Tooltip("Yakalama şansına eklenecek değer (-0.3 ile 0.3 aralığı)")]
    [Range(-0.3f, 0.3f)] public float catchMod; // yakalama şansına eklenir

    [Tooltip("Canlı yakalandığında satış fiyatı")]
    public int alivePrice;

    [Tooltip("Ölü yakalandığında satış fiyatı")]
    public int deadPrice;

    public DragonType(string name, int rarity, float catchMod, int alivePrice, int deadPrice)
    {
        this.name = name;
        this.rarity = rarity;
        this.catchMod = catchMod;
        this.alivePrice = alivePrice;
        this.deadPrice = deadPrice;
    }
}

