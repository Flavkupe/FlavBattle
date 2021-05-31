using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Linq;

public class Detector : MonoBehaviour
{
    public DetectableType Detects;

    public event EventHandler<GameObject> Detected;
    public event EventHandler<GameObject> Exited;
    public event EventHandler<MouseButton> Clicked;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var other = collision.GetComponent<IDetectable>();
        if (other != null && other.Type == this.Detects)
        {
            Detected?.Invoke(this, other.GetObject());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var other = collision.GetComponent<IDetectable>();
        if (other != null && other.Type == this.Detects)
        {
            Exited?.Invoke(this, other.GetObject());
        }
    }

    private void OnMouseOver()
    {
        if (Detects.HasFlag(DetectableType.MouseClick))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Clicked?.Invoke(this, MouseButton.LeftButton);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Clicked?.Invoke(this, MouseButton.RightButton);
            }
        }
    }

    public T[] GetDetected<T>() where T : MonoBehaviour
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return GetDetected<T>(boxCollider);
        }

        var circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            return GetDetected<T>(circleCollider);
        }

        var polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            return GetDetected<T>(polyCollider);
        }

        // no colliders; empty list
        Debug.LogWarning("Trying to detect with no colliders");
        return new T[] { };
    }

    public GameObject[] GetDetected()
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return GetDetected(boxCollider);
        }

        var circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            return GetDetected(circleCollider);
        }

        // no colliders; empty list
        Debug.LogWarning("Trying to detect with no colliders");
        return new GameObject[] { };
    }

    public GameObject[] GetDetected(BoxCollider2D collider)
    {
        var all = Physics2D.OverlapBoxAll(transform.position + collider.offset.ToVector3(), collider.size, 0);
        return all.Select(b => b.gameObject).ToArray();
    }

    public GameObject[] GetDetected(CircleCollider2D collider)
    {
        var all = Physics2D.OverlapCircleAll(transform.position + collider.offset.ToVector3(), collider.radius);
        return all.Select(b => b.gameObject).ToArray();
    }

    public T[] GetDetected<T>(BoxCollider2D collider) where T : MonoBehaviour
    {
        var all = Physics2D.OverlapBoxAll(transform.position + collider.offset.ToVector3(), collider.size, 0);
        return all.Select(a => a.gameObject.GetComponentInParent<T>()).Where(b => b != null).ToArray();
    }

    public T[] GetDetected<T>(CircleCollider2D collider) where T : MonoBehaviour
    {
        var all = Physics2D.OverlapCircleAll(transform.position + collider.offset.ToVector3(), collider.radius);
        return all.Select(a => a.gameObject.GetComponentInParent<T>()).Where(b => b != null).ToArray();
    }

    public T[] GetDetected<T>(PolygonCollider2D collider) where T : MonoBehaviour
    {
        List<Collider2D> results = new List<Collider2D>();
        var filter = new ContactFilter2D();
        var numResults = Physics2D.OverlapCollider(collider, filter.NoFilter(), results);
        if (results == null || numResults == 0)
        {
            return new T[] { };
        }

        return results.Select(a => a.gameObject.GetComponentInParent<T>()).Where(b => b != null).ToArray();
    }
}

public static class DetectableExtensions
{
    public static T[] GetDetected<T>(this MonoBehaviour obj, DetectableType detectorFilter) where T : MonoBehaviour
    {
        var all = new List<T>();
        foreach (var a in obj.GetComponentsInChildren<Detector>().Where(a => a.Detects == detectorFilter))
        {
            all.AddRange(a.GetDetected<T>());
        }

        return all.ToArray();
    }

    public static GameObject[] GetDetected(this MonoBehaviour obj, DetectableType detectorFilter)
    {
        var all = new List<GameObject>();
        foreach (var a in obj.GetComponentsInChildren<Detector>().Where(a => a.Detects == detectorFilter))
        {
            all.AddRange(a.GetDetected());
        }

        return all.ToArray();
    }

    /// <summary>
    /// Returns true if this detects the target, and false otherwise
    /// </summary>
    public static bool Detects(this MonoBehaviour obj, IDetectable target)
    {
        foreach (var a in obj.GetDetected(target.Type))
        {
            if (a.gameObject == target.GetObject())
            {
                return true;
            }
        }

        return false;
    }

    public static T[] GetDetected<T>(this MonoBehaviour obj) where T : MonoBehaviour
    {
        var all = new List<T>();
        foreach (var a in obj.GetComponentsInChildren<Detector>())
        {
            all.AddRange(a.GetDetected<T>());
        }

        return all.ToArray();
    }
}
