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

        var random = Random.Range(0, list.Count);
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

    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static IEnumerator MoveBy(this MonoBehaviour obj, Vector3 movement, float speed)
    {
        var target = obj.transform.position + movement;
        var distLeft = movement.magnitude;
        while (distLeft > 0.0f)
        {
            var step = speed * Time.deltaTime;
            distLeft -= step;
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, step);
            yield return null;
        }

        obj.transform.position = target;
    }

    public static IEnumerator FlashColor(this SpriteRenderer sprite, Color color, float speed = 8.0f)
    {
        var starting = sprite.color;
        var progress = 0.0f;
        while (progress < 1.0f)
        {
            progress += speed * Time.deltaTime;
            sprite.color = Color.Lerp(sprite.color, color, progress);
            yield return null;
        }

        progress = 0.0f;
        while (progress < 1.0f)
        {
            progress += speed * Time.deltaTime;
            sprite.color = Color.Lerp(sprite.color, starting, progress);
        }

        sprite.color = starting;
    }
}
