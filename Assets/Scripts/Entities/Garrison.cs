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

    void Start()
    {
        this.transform.position = this.transform.position - new Vector3(0.0f, 0.25f);
    }
}
