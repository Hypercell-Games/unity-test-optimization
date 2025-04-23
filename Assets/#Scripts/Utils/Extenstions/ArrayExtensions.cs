using System;

public static class ArrayExtensions
{
    public static T Random<T>(this T[] array)
    {
        if (array.Length == 0)
        {
            return default;
        }

        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static T Random<T>(this T[] array, Random random)
    {
        if (array.Length == 0)
        {
            return default;
        }

        return array[random.Next(0, array.Length)];
    }

    public static void Shuffle<T>(this T[] array)
    {
        var rng = new Random();
        var n = array.Length;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }

    public static void For<T>(this T[] array, Action<T> action)
    {
        var n = array.Length;
        for (var i = 0; i < n; i++)
        {
            var a = array[i];
            action?.Invoke(a);
        }
    }

    public static void For<T>(this T[] array, Action<T, int> action)
    {
        var n = array.Length;
        for (var i = 0; i < n; i++)
        {
            var a = array[i];
            action?.Invoke(a, i);
        }
    }
}
