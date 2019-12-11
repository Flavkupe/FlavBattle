using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public bool Passable;
    public float WalkCost = 1.0f;

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
