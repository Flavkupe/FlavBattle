using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum DetectableType
{
    Army = 1,
    Town = 2,
}

public interface IDetectable
{
    GameObject GetObject();
    DetectableType Type { get; }
}
