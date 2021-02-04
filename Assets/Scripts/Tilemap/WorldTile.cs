using FlavBattle.Tilemap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WorldTile", menuName = "Custom/Tiles/World Tile", order = 1)]
public class WorldTile : Tile
{
    [SerializeField]
    private FlavBattle.Tilemap.TileData _data;

    public TileInfo Info => _data?.Info;

    [AssetIcon]
    public Sprite Icon => this.sprite;
}

