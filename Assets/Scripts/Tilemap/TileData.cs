using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialTileProperty
{
    None = 0,
    Garrison = 1,
    Town = 2,
}

[Serializable]
public class TileData
{
    public bool Passable;
    public float WalkCost = 1.0f;

    public SpecialTileProperty SpecialProperty;

    /// <summary>
    /// If true, then the tile is passable no matter what else
    /// is here. Bridges are a good example.
    /// </summary>
    public bool OverridePassable = false;

    public TileData Combine(TileData data)
    {
        return new TileData()
        {
            Passable = OverridePassable || data.OverridePassable || (Passable && data.Passable),
            WalkCost = WalkCost + data.WalkCost
        };
    }
}
