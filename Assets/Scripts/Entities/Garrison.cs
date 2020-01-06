using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garrison : MonoBehaviour, IDetectable
{
    public DetectableType Type => DetectableType.Tile;

    public GameObject GetObject()
    {
        return this.gameObject;
    }
}
