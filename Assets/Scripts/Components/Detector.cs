using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Linq;
using FlavBattle.Components;

/// <summary>
/// Detectors attempt to detect other objects in range that
/// match the DetectableType.
/// 
/// NOTE: Detectors only raycast to objects in the Detectable Layer!
/// They should also implement IDetectable.
/// </summary>
public class Detector : MonoBehaviour
{
    public DetectableType Detects;

    [Required]
    [Tooltip("The root object that owns this detector, such as the Army. Used to ensure detectors from same owner don't detect eachother.")]
    [SerializeField]
    private GameObject _owner;

    public event EventHandler<GameObject> Detected;
    public event EventHandler<GameObject> Exited;
    public event EventHandler<MouseButton> Clicked;

    private List<ITrackableObject> _trackedObjects = new List<ITrackableObject>();

    private void Start()
    {
        // Invoke(nameof(InitDetection), 0.5f);
        foreach (var item in GetDetected())
        {
            HandleEntered(item.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = GetObject(collision);
        if (obj == null)
        {
            return;
        }

        HandleEntered(obj);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var obj = GetObject(collision);
        if (obj == null)
        {
            return;
        }
        
        Exited?.Invoke(this, obj);

        var trackable = obj.GetComponent<ITrackableObject>();
        if (trackable != null)
        {
            _trackedObjects.Remove(trackable);
            trackable.Destroyed -= HandleTrackableDestroyed;
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

    private void HandleEntered(GameObject obj, bool fromInit = false)
    {
        if (obj == null)
        {
            Logger.Warning(LogType.State, "Null object detected");
            return;
        }

        Detected?.Invoke(this, obj);

        var trackable = obj.GetComponent<ITrackableObject>();
        if (trackable != null)
        {
            _trackedObjects.Add(trackable);
            trackable.Destroyed += HandleTrackableDestroyed;
        }

        if (fromInit)
        {
            Logger.Log(LogType.State, $"[{this.name}] detecting [{obj.name}] from initialization", this.gameObject);
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
        e.Destroyed -= HandleTrackableDestroyed;
        _trackedObjects.Remove(e);
        Exited?.Invoke(sender, e.GetObject());
    }

    /// <summary>
    /// Grabs a GameObject from an IDetectable if it's a valid one.
    /// If the collision is not valid, returns null.
    /// </summary>
    private GameObject GetObject(Collider2D collision)
    {
        var other = collision?.GetComponent<IDetectable>();
        if (other == null || other.Type != this.Detects)
        {
            return null;
        }

        var obj = other.GetObject();
        if (obj == this._owner)
        {
            // detected object with same owner
            return null;
        }

        return obj;
    }

    public T[] GetDetected<T>() where T : Component
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

        var polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            return GetDetected(polyCollider);
        }

        // no colliders; empty list
        Debug.LogWarning("Trying to detect with no colliders");
        return new GameObject[] { };
    }

    public GameObject[] GetDetected(BoxCollider2D collider)
    {
        var mask = GetDetectableLayerMask();
        var all = Physics2D.OverlapBoxAll(transform.position + collider.offset.ToVector3(), collider.size, 0, mask.value);
        return FilterDetectables(all);
    }

    public GameObject[] GetDetected(CircleCollider2D collider)
    {
        var mask = GetDetectableLayerMask();
        var all = Physics2D.OverlapCircleAll(transform.position + collider.offset.ToVector3(), collider.radius, mask.value);
        return FilterDetectables(all);
    }

    public GameObject[] GetDetected(PolygonCollider2D collider)
    {
        List<Collider2D> results = new List<Collider2D>();
        var filter = new ContactFilter2D();
        var layer = GetDetectableLayerMask();
        filter.SetLayerMask(layer);
        filter.useTriggers = true;
        var numResults = Physics2D.OverlapCollider(collider, filter, results);
        if (results == null || numResults == 0)
        {
            return new GameObject[] { };
        }

        return FilterDetectables(results);
    }

    public T[] GetDetected<T>(BoxCollider2D collider) where T : Component
    {
        var mask = GetDetectableLayerMask();
        var all = Physics2D.OverlapBoxAll(transform.position + collider.offset.ToVector3(), collider.size, 0, mask.value);
        return FilterDetectables<T>(all);
    }

    public T[] GetDetected<T>(CircleCollider2D collider) where T : Component
    {
        var mask = GetDetectableLayerMask();
        var all = Physics2D.OverlapCircleAll(transform.position + collider.offset.ToVector3(), collider.radius, mask.value);
        // return all.Select(a => a.gameObject.GetComponentInParent<T>()).Where(b => b != null).ToArray();
        return FilterDetectables<T>(all);
    }

    public T[] GetDetected<T>(PolygonCollider2D collider) where T : Component
    {
        List<Collider2D> results = new List<Collider2D>();
        var filter = new ContactFilter2D();
        var mask = GetDetectableLayerMask();
        filter.SetLayerMask(mask);
        filter.useTriggers = true;
        var numResults = Physics2D.OverlapCollider(collider, filter, results);
        if (results == null || numResults == 0)
        {
            return new T[] { };
        }

        // return results.Select(a => a.gameObject.GetComponentInParent<T>()).Where(b => b != null).ToArray();
        return FilterDetectables<T>(results);
    }

    /// <summary>
    /// Filter results to only get stuff that should be detected by this Detector.
    /// </summary>
    private T[] FilterDetectables<T>(IEnumerable<Collider2D> detected) where T : Component
    {
        var valid = new List<T>();
        foreach (var item in detected)
        {
            var detectable = item.GetComponent<IDetectable>();
            if (detectable == null || detectable.Type != this.Detects)
            {
                continue;
            }

            var obj = detectable.GetObject<T>();
            if (obj != null)
            {
                valid.Add(obj);
            }
        }

        return valid.ToArray();
    }

    /// <summary>
    /// Filter results to only get stuff that should be detected by this Detector.
    /// </summary>
    private GameObject[] FilterDetectables(IEnumerable<Collider2D> detected)
    {
        var valid = new List<GameObject>();
        foreach (var item in detected)
        {
            var detectable = item.GetComponent<IDetectable>();
            if (detectable == null || detectable.Type != this.Detects)
            {
                continue;
            }

            var obj = detectable.GetObject();
            if (obj != null)
            {
                valid.Add(obj);
            }
        }

        return valid.ToArray();
    }

    private LayerMask GetDetectableLayerMask()
    {
        return LayerMask.GetMask("Detectable");
    }
}

public static class DetectableExtensions
{
    /// <summary>
    /// Gets all stuff detected by this object's Detectors, within bounds of filter.
    /// </summary>
    public static T[] GetAllDetected<T>(this MonoBehaviour obj, DetectableType detectorFilter) where T : MonoBehaviour
    {
        var all = new List<T>();
        foreach (var a in obj.GetComponentsInChildren<Detector>().Where(a => a.Detects == detectorFilter))
        {
            all.AddRange(a.GetDetected<T>());
        }

        return all.ToArray();
    }

    /// <summary>
    /// Gets all stuff detected by this object's Detectors, within bounds of filter.
    /// </summary>
    public static GameObject[] GetAllDetected(this MonoBehaviour obj, DetectableType detectorFilter)
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
        foreach (var a in obj.GetAllDetected(target.Type))
        {
            if (a.gameObject == target.GetObject())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all stuff detected by this object's Detectors.
    /// </summary>
    public static T[] GetAllDetected<T>(this MonoBehaviour obj) where T : MonoBehaviour
    {
        var all = new List<T>();
        foreach (var a in obj.GetComponentsInChildren<Detector>())
        {
            all.AddRange(a.GetDetected<T>());
        }

        return all.ToArray();
    }
}
