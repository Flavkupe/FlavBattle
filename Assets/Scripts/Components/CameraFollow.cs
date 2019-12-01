using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Range(0.75f, 0.95f)]
    public float EdgeThreshold = 0.9f;

    public float Speed = 3.0f;

    Camera _cam;

    private Bounds _bounds;
    private bool _boundsSet = false;


    public void SetBounds(Bounds bounds)
    {
        _boundsSet = true;
        _bounds = bounds;
    }

    void Start()
    {
        _cam = GetComponent<Camera>();
    }

    void Update()
    {
        var xPercent = Input.mousePosition.x / Screen.width;
        var yPercent = Input.mousePosition.y / Screen.height;

        var xTravel = GetAxisSpeed(xPercent);
        var yTravel = GetAxisSpeed(yPercent);

        if (xTravel != 0.0f || yTravel != 0.0f)
        {
            transform.position += new Vector3(xTravel, yTravel, 0.0f);
            if (_boundsSet)
            {
                var x = Mathf.Clamp(transform.position.x, _bounds.min.x, _bounds.max.x);
                var y = Mathf.Clamp(transform.position.y, _bounds.min.y, _bounds.max.y);
                transform.position = new Vector3(x, y, transform.position.z);
            }
        }
    }

    private float GetAxisSpeed(float axisPercent)
    {
        var rate = Time.deltaTime * this.Speed;
        if (axisPercent > EdgeThreshold)
        {
            return rate;
        }
        else if (axisPercent < 1.0f - EdgeThreshold)
        {
            return -rate;
        }

        return 0.0f;
    }
}
