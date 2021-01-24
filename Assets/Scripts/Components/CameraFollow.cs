using FlavBattle.State;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Range(0.75f, 1.00f)]
    public float EdgeThreshold = 0.95f;

    private bool ShowOffscreenScrollingControls() => StopScrollingPastScreen;
    [Tooltip("If true, camera will not keep scrolling if the mouse is off the game screen")]
    public bool StopScrollingPastScreen = true;
    [Tooltip("How far past the screen to keep scrolling. Only used if StopScrollingPastScreen is true")]
    [ShowIf("ShowOffscreenScrollingControls")]
    public float OffscreenPadding = 0.1f;

    public float Speed = 3.0f;
    public float MiddlePanSpeed = 6.0f;

    private Vector3? _lastMiddleClickPos;

    [Tooltip("Amount of padding for scroll limits, past outermost extents of tilemap (in world units)")]
    public float EdgePadding = 1.0f;

    Camera _cam;

    private Bounds _bounds;
    private bool _boundsSet = false;
    private bool _paused = false;

    public void SetBounds(Bounds bounds)
    {
        _boundsSet = true;
        _bounds = bounds;
    }

    void Start()
    {
        _cam = GetComponent<Camera>();
        var events = FindObjectOfType<GameEventManager>();
        events.MapEvent += Events_MapEvent;
    }

    private void Events_MapEvent(object sender, MapEventType e)
    {
        switch (e)
        {
            case MapEventType.MapPaused:
                _paused = true;
                return;
            case MapEventType.MapUnpaused:
                _paused = false;
                return;
        }
    }

    void Update()
    {
        if (_paused)
        {
            return;
        }

        var xTravel = 0.0f;
        var yTravel = 0.0f;

        if (Input.GetMouseButtonUp(2))
        {
            _lastMiddleClickPos = null;
        }
        else if (Input.GetMouseButton(2))
        {
            MiddleMouseMovement(ref xTravel, ref yTravel);
        }
        else
        {
            KeyboardMouseMove(ref xTravel, ref yTravel);
        }

        if (xTravel == 0.0f && yTravel == 0.0f)
        {
            EdgeMouseMovement(ref xTravel, ref yTravel);
        }

        if (xTravel != 0.0f || yTravel != 0.0f)
        {
            MoveMap(xTravel, yTravel);
        }
    }

    private void MiddleMouseMovement(ref float xTravel, ref float yTravel)
    {
        var rate = Time.deltaTime * this.MiddlePanSpeed;
        if (_lastMiddleClickPos == null)
        {
            _lastMiddleClickPos = Input.mousePosition;
        }
        else
        {
            var mouse = Input.mousePosition;
            if (_lastMiddleClickPos.Value.x < mouse.x)
            {
                xTravel -= rate;
            }
            else if (_lastMiddleClickPos.Value.x > mouse.x)
            {
                xTravel += rate;
            }

            if (_lastMiddleClickPos.Value.y < mouse.y)
            {
                yTravel -= rate;
            }
            else if (_lastMiddleClickPos.Value.y > mouse.y)
            {
                yTravel += rate;
            }

            _lastMiddleClickPos = Input.mousePosition;
        }
    }

    private void KeyboardMouseMove(ref float xTravel, ref float yTravel)
    {
        var rate = Time.deltaTime * this.Speed;
        if (Input.GetKey(KeyCode.A))
        {
            xTravel -= rate;
        }
        if (Input.GetKey(KeyCode.D))
        {
            xTravel += rate;
        }
        if (Input.GetKey(KeyCode.S))
        {
            yTravel -= rate;
        }
        if (Input.GetKey(KeyCode.W))
        {
            yTravel += rate;
        }
    }

    private void EdgeMouseMovement(ref float xTravel, ref float yTravel)
    {
        // Not pressing keys; use mouse on edge
        var xPercent = Input.mousePosition.x / Screen.width;
        var yPercent = Input.mousePosition.y / Screen.height;

        xTravel = GetAxisSpeed(xPercent);
        yTravel = GetAxisSpeed(yPercent);
    }


    private void MoveMap(float xTravel, float yTravel)
    {
        transform.position += new Vector3(xTravel, yTravel, 0.0f);
        if (_boundsSet)
        {
            var camHeight = _cam.orthographicSize;
            var camWidth = _cam.aspect * camHeight;

            var minX = _bounds.min.x + camWidth - EdgePadding;
            var maxX = _bounds.max.x - camWidth + EdgePadding;
            var minY = _bounds.min.y + camHeight - EdgePadding;
            var maxY = _bounds.max.y - camHeight + EdgePadding;
            var x = Mathf.Clamp(transform.position.x, minX, maxX);
            var y = Mathf.Clamp(transform.position.y, minY, maxY);
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    private float GetAxisSpeed(float axisPercent)
    {
        if (StopScrollingPastScreen && (axisPercent < (0.0f - OffscreenPadding) || axisPercent > (1.0f + OffscreenPadding)))
        {
            return 0.0f;
        }

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
