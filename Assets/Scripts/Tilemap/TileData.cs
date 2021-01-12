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

[CreateAssetMenu(fileName = "Tile Data", menuName = "Custom/Tiles/Tile Data", order = 1)]
public class TileData : ScriptableObject
{
    public TileInfo Info;
}

[Serializable]
public class TileInfo
{
    public bool Passable;
    public float WalkCost = 1.0f;

    public SpecialTileProperty SpecialProperty;

    /// <summary>
    /// If true, then the tile is passable no matter what else
    /// is here. Bridges are a good example.
    /// </summary>
    public bool OverridePassable = false;

    public TileInfo Combine(TileInfo data)
    {
        return new TileInfo()
        {
            Passable = OverridePassable || data.OverridePassable || (Passable && data.Passable),
            WalkCost = WalkCost + data.WalkCost
        };
    }
}
