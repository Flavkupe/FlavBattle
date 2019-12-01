using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class Detector : MonoBehaviour
{
    public DetectableType Detects;

    public event EventHandler<GameObject> Detected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var other = collision.GetComponent<IDetectable>();
        if (other != null && other.Type == this.Detects)
        {
            Detected?.Invoke(this, other.GetObject());
        }
    }
}
