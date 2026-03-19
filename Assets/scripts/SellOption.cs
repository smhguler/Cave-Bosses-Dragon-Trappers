/// <summary>
/// Satış için seçilebilir çeşitleri temsil eden enum.
/// </summary>
public enum SellItemKind
{
    Trap,
    Rabbit,
    DragonAlive,
    DragonDead
}

/// <summary>
/// Satış ekranındaki tek bir seçeneği temsil eden veri sınıfı.
/// Ejderha dışındaki türler için sadece kind alanı kullanılır.
/// </summary>
[System.Serializable]
public class SellOption
{
    public SellItemKind kind;

    /// <summary>
    /// Sadece ejderha satışı için kullanılan isim.
    /// Diğer türler için boş kalabilir.
    /// </summary>
    public string dragonName;
}

