public interface IGameRandom
{
    float Value();
    int Range(int minInclusive, int maxExclusive);
}

public sealed class UnityGameRandom : IGameRandom
{
    public float Value()
    {
        return UnityEngine.Random.value;
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return minInclusive;
        }

        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }
}

public sealed class SeededGameRandom : IGameRandom
{
    private readonly System.Random random;

    public SeededGameRandom(int seed)
    {
        random = new System.Random(seed);
    }

    public float Value()
    {
        return (float)random.NextDouble();
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return minInclusive;
        }

        return random.Next(minInclusive, maxExclusive);
    }
}