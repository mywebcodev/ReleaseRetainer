namespace ReleaseRetainer.Test.Helpers;

public static class TestDataGenerator
{
    private static readonly Random Random = new();

    public static int GetRandomNumber(int minValue, int maxValue)
    {
        // Ensure thread safety if used in a multi-threaded environment
        lock (Random)
        {
            return Random.Next(minValue, maxValue);
        }
    }
}