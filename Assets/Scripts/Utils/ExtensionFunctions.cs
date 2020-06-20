using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class ExtensionFunctions
{
    public static TValue GetRandom<TValue>(this IList<TValue> list)
    {
        if (list.Count == 0)
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

    public static Vector3 ShiftX(this Vector3 vector, float x)
    {
        return new Vector3(vector.x + x, vector.y, vector.z);
    }

    public static Vector3 ShiftY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y + y, vector.z);
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

    public static bool HasComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        return obj.GetComponent<T>();
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
}
