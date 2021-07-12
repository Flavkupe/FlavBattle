using FlavBattle.Components;
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
    MouseClick = 4,
}

/// <summary>
/// Interface for an object that can be detected by a Detector.
/// </summary>
public interface IDetectable : IHasGameObject
{
    DetectableType Type { get; }
}
