﻿using System.Collections;
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

    public static Color SetAlpha(this Color color, float a)
    {
        return new Color(color.r, color.g, color.b, a);
    }

    public static IEnumerator MoveTo(this MonoBehaviour obj, Vector3 target, float speed = 10.0f)
    {
        var distLeft = Vector3.Distance(obj.transform.position, target);
        target = target.SetZ(obj.transform.position.z);
        while (distLeft > 0.0f)
        {
            var step = speed * Time.deltaTime;
            distLeft -= step;
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, step);
            yield return null;
        }

        obj.transform.position = target;
    }

    public static IEnumerator MoveBy(this MonoBehaviour obj, Vector3 movement, float speed = 10.0f)
    {
        var target = obj.transform.position + movement;
        yield return MoveTo(obj, target, speed);
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

    public static IEnumerator FadeAway(this MonoBehaviour obj, float speed = 1.0f)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning("No renderer! Can't fade away");
            yield break;
        }

        yield return FadeAway(renderer, speed);
    }

    public static IEnumerator FadeAway(this SpriteRenderer sprite, float speed = 1.0f)
    {
        var starting = sprite.color;
        while (sprite.color.a > 0.0f)
        {
            var newAlpha = sprite.color.a - (speed * Time.deltaTime);
            sprite.color = sprite.color.SetAlpha(newAlpha);
            yield return null;
        }
    }
}
