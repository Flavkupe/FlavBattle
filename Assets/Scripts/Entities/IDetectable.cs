using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum DetectableType
{
    None = 0,
    Army = 1,
    Tile = 2,
}

public interface IDetectable
{
    GameObject GetObject();
    DetectableType Type { get; }
}
