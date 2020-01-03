using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class Detector : MonoBehaviour
{
    public DetectableType Detects;

    public event EventHandler<GameObject> Detected;
    public event EventHandler<GameObject> Exited;

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
}
