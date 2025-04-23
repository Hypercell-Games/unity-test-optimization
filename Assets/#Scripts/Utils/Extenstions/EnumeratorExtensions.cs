using System.Collections;
using System.Collections.Generic;

public static class EnumeratorExtensions
{
    public static IEnumerator<T> Cast<T>(this IEnumerator iterator)
    {
        while (iterator.MoveNext())
        {
            yield return (T)iterator.Current;
        }
    }
}
