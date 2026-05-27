public enum SellItemKind
{
    Trap,
    Rabbit,
    DragonAlive,
    DragonDead
}

[System.Serializable]
public class SellOption
{
    public SellItemKind kind;
    public string dragonName;
}