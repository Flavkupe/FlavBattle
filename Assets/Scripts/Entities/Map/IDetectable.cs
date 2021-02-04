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

public static class IDetectableExtensions
{
    public static T GetComponent<T>(this IDetectable obj) where T : Component 
    {
        return obj.GetObject()?.GetComponent<T>();
    }
}