using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionFunctions
{
    public static TValue GetRandom<TValue>(this IList<TValue> list) where TValue : class
    {
        if (list.Count == 0)
        {
            return null;
        }

        var random = Random.Range(0, list.Count - 1);
        return list[random];
    }

    /// <summary>
    /// Shuffles the provided list. Returns the same instance of the list, shuffled.
    /// </summary>
    public static IList<TValue> GetShuffled<TValue>(this IList<TValue> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var randomIndex = Random.Range(0, list.Count - 1);
            var temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }
}
