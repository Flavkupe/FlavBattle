using FlavBattle.Tilemap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PropsTile", menuName = "Custom/Tiles/Props Tile", order = 2)]
public class PropsTile : Tile
{
    [SerializeField]
    private FlavBattle.Tilemap.TileData _data;

    public TileInfo Info => _data?.Info;

    [AssetIcon]
    public Sprite Icon => this.sprite;
}
