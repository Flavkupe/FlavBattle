using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Linq;
using FlavBattle.Components;

public class Detector : MonoBehaviour
{
    public DetectableType Detects;

    public event EventHandler<GameObject> Detected;
    public event EventHandler<GameObject> Exited;
    public event EventHandler<MouseButton> Clicked;

    private List<ITrackableObject> _trackedObjects = new List<ITrackableObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var other = collision.GetComponent<IDetectable>();
        if (other != null && other.Type == this.Detects)
        {
            var obj = other.GetObject();
            Detected?.Invoke(this, other.GetObject());

            var trackable = obj.GetComponent<ITrackableObject>();
            if (trackable != null)
            {
                _trackedObjects.Add(trackable);
                trackable.Destroyed += HandleTrackableDestroyed;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var other = collision.GetComponent<IDetectable>();
        if (other != null && other.Type == this.Detects)
        {
            var obj = other.GetObject();
            Exited?.Invoke(this, obj);

            var trackable = obj.GetComponent<ITrackableObject>();
            if (trackable != null)
            {
                _trackedObjects.Remove(trackable);
                trackable.Destroyed -= HandleTrackableDestroyed;
            }
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

    /// <summary>
    /// Handles situation where trackable object becomes destroyed before
    /// exiting the Detector
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HandleTrackableDestroyed(object sender, ITrackableObject e)
    {
        _trackedObjects.Remove(e);
        Exited.Invoke(sender, e.GetObject());
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
