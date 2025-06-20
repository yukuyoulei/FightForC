internal static class RandomHelper
{
    private static readonly System.Random random = new System.Random();
    public static int Next(int max)
    {
        return random.Next(0, max);
    }
    public static int Next(int min, int max)
    {
        return random.Next(min, max);
    }
    public static float NextFloat(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }
}
