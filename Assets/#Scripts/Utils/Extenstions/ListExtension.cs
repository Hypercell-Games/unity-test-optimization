#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

public static class ListExtensions
{
    public static T Random<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            return default;
        }

        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static T RandomRemove<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            return default;
        }

        var at = UnityEngine.Random.Range(0, list.Count);
        var item = list[at];
        list.RemoveAt(at);
        return item;
    }

    public static void RemoveAll<T>(this List<T> list, List<T> other)
    {
        if (list.Count == 0 || list == other)
        {
            return;
        }

        foreach (var item in other)
        {
            list.Remove(item);
        }
    }

    public static void Shuffle<T>(this List<T> list)
    {
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static List<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}
