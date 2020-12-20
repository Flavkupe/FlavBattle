using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldTile", menuName = "Custom/Tiles/World Tile", order = 1)]
public class WorldTile : Tile
{
    public TileData TileData;

    [AssetIcon]
    public Sprite Icon => this.sprite;
}

