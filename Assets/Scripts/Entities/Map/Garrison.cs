using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Garrison : MonoBehaviour, IDetectable
{
    public DetectableType Type => DetectableType.Tile;

    public event EventHandler RightClicked;

    public GameObject GetObject()
    {
        return this.gameObject;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RightClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
