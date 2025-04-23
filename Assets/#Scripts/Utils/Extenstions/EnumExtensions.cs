using System;
using UnityEngine;

public static class EnumExtensions
{
    public static T Next<T>(this T src) where T : Enum
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        var Arr = (T[])Enum.GetValues(src.GetType());
        var j = Array.IndexOf(Arr, src) + 1;
        return j == Arr.Length ? Arr[0] : Arr[j];
    }

    public static T Prev<T>(this T src) where T : Enum
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        var Arr = (T[])Enum.GetValues(src.GetType());
        var j = Array.IndexOf(Arr, src) - 1;
        return j < 0 ? Arr[Arr.Length - 1] : Arr[j];
    }

    public static T Get<T>(this T src, int ind) where T : Enum
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        var Arr = (T[])Enum.GetValues(src.GetType());
        ind = Mathf.Clamp(ind, 0, Arr.Length - 1);
        return Arr[ind];
    }
}
