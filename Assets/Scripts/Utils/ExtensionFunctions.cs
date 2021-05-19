using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class ExtensionFunctions
{
    public static TValue GetRandom<TValue>(this IList<TValue> list)
    {
        if (list == null || list.Count == 0)
        {
            return default(TValue);
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

    public static R GetValueOrDefault<T, R>(this IDictionary<T, R> dict, T key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }

        return default;
    }

    public static void SetOrAddTo<T>(this IDictionary<T, int> dict, T key, int val)
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = val;
        }
        else
        {
            dict[key] += val;
        }
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

    public static Vector3 SetXY(this Vector3 vector, float x, float y)
    {
        return new Vector3(x, y, vector.z);
    }

    public static Vector3 ShiftX(this Vector3 vector, float x)
    {
        return new Vector3(vector.x + x, vector.y, vector.z);
    }

    public static Vector3 ShiftY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y + y, vector.z);
    }

    public static Vector2 Shift(this Vector2 vector, float x, float y)
    {
        return new Vector2(vector.x + x, vector.y + y);
    }

    public static Color SetAlpha(this Color color, float a)
    {
        return new Color(color.r, color.g, color.b, a);
    }

    public static IEnumerator MoveTo(this MonoBehaviour obj, Vector3 target, float speed = 10.0f, AccelOption accel = AccelOption.None)
    {
        var distLeft = Vector3.Distance(obj.transform.position, target);
        target = target.SetZ(obj.transform.position.z);
        while (distLeft > 0.0f)
        {
            var step = speed * TimeUtils.GameSpeed.GetAdjustedDeltaTime(accel);
            distLeft -= step;
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, step);
            yield return null;
        }

        obj.transform.position = target;
    }

    public static IEnumerator MoveBy(this MonoBehaviour obj, Vector3 movement, float speed = 10.0f, AccelOption accel = AccelOption.None)
    {
        var target = obj.transform.position + movement;
        yield return MoveTo(obj, target, speed, accel);
    }

    public static IEnumerator FlashColor(this SpriteRenderer sprite, Color color, float speed = 8.0f)
    {
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
            sprite.color = Color.Lerp(sprite.color, Color.white, progress);
        }

        sprite.color = Color.white;
    }

    public static IEnumerator FadeAway(this MonoBehaviour obj, float speed = 1.0f, AccelOption accel = AccelOption.None)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning("No renderer! Can't fade away");
            yield break;
        }

        yield return FadeAway(renderer, speed, accel);
    }

    public static IEnumerator FadeAway(this SpriteRenderer sprite, float speed = 1.0f, AccelOption accel = AccelOption.None)
    {
        var starting = sprite.color;
        while (sprite.color.a > 0.0f)
        {
            var newAlpha = sprite.color.a - (speed * TimeUtils.GameSpeed.GetAdjustedDeltaTime(accel));
            sprite.color = sprite.color.SetAlpha(newAlpha);
            yield return null;
        }
    }

    /// <summary>
    /// Calls GetComponent if val is null, caching it on val. Otherwise returns val.
    /// Useful as a shortcut to property getters that cache the value from a GetComponent.
    /// </summary>
    public static T GetCachedComponent<T>(this MonoBehaviour obj, ref T val)
    {
        if (val == null)
        {
            val = obj.GetComponent<T>();
        }

        return val;
    }

    public static bool Matches(this IFormationGridSlot slot, FormationPair pair)
    {
        return slot.Col == pair.Col && slot.Row == pair.Row;
    }

    public static bool Matches(this IFormationGridSlot slot, FormationRow row, FormationColumn col)
    {
        return slot.Col == col && slot.Row == row;
    }

    public static void Show(this MonoBehaviour obj)
    {
        obj.gameObject.SetActive(true);
    }

    public static bool IsShowing(this MonoBehaviour obj)
    {
        return obj.gameObject.activeInHierarchy;
    }

    public static void Hide(this MonoBehaviour obj)
    {
        obj.gameObject.SetActive(false);
    }

    public static void Hide(this GameObject obj)
    {
        obj.SetActive(false);
    }

    public static void Show(this GameObject obj)
    {
        obj.SetActive(true);
    }

    public static void SetActive(this MonoBehaviour obj, bool active)
    {
        obj.gameObject.SetActive(active);
    }

    public static void ToggleActive(this MonoBehaviour obj)
    {
        obj.gameObject.SetActive(!obj.gameObject.activeInHierarchy);
    }

    public static bool HasComponent<T>(this GameObject obj)
    {
        return obj.GetComponent<T>() != null;
    }

    public static Vector3 ToVector3(this Vector2 vect)
    {
        return new Vector3(vect.x, vect.y);
    }

    public static Vector2 ToVector2(this Vector3 vect)
    {
        return new Vector2(vect.x, vect.y);
    }

    public static int RandomBetween(this Vector2 range)
    {
        return Random.Range((int)range.x, (int)range.y);
    }

    public static void DoAfter(this MonoBehaviour behavior, float seconds, System.Action action)
    {
        behavior.StartCoroutine(DoAfterInternal(seconds, action));
    }

    private static IEnumerator DoAfterInternal(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

    /// <summary>
    /// Gets the SpriteRenderer or Image and sets the color of that.
    /// First tries to get a SpriteRenderer, and get Image if that's null.
    /// </summary>
    public static void SetColor(this MonoBehaviour obj, Color color)
    {
        var rend = obj.GetComponent<SpriteRenderer>();
        if (rend != null)
        {
            rend.color = color;
        }
        else
        {
            var img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }
            else
            {
                Debug.LogWarning($"No SpriteRenderer or Image component on {obj.name}");
            }
        }
    }

    public static void DestroyChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (child != null)
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Gets the item with the highest value based on the selector. It's like
    /// Linq's Max but it gets the item rather than the value from the selector.
    /// </summary>
    public static T GetMax<T, R>(this IList<T> items, System.Func<T, R> selector) where R : System.IComparable
    {
        return items.GetCompare(selector, 1);
    }

    /// <summary>
    /// Gets the item with the smallest value based on the selector. It's like
    /// Linq's Min but it gets the item rather than the value from the selector.
    /// </summary>
    public static T GetMin<T, R>(this IList<T> items, System.Func<T, R> selector) where R : System.IComparable
    {
        return items.GetCompare(selector, -1);
    }

    /// <summary>
    /// Compares items with CompareTo based on compareVal (-1 for less than, 1 for
    /// greater than, 0 for equal, etc). Use for stuff like GetMin, GetMax, etc.
    /// </summary>
    private static T GetCompare<T, R>(this IList<T> items, System.Func<T, R> selector, int compareVal) where R : System.IComparable
    {
        if (items.Count == 0)
        {
            return default(T);
        }

        var maxIndex = 0;
        for (var i = 1; i < items.Count; i++)
        {
            var max = selector(items[maxIndex]);
            var curr = selector(items[i]);
            if (curr.CompareTo(max) == compareVal)
            {
                maxIndex = i;
            }
        }

        return items[maxIndex];
    }
}
