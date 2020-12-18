﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTile
{
    public TileData Data;
    public int GridX;
    public int GridY;
    public float WorldX;
    public float WorldY;

    public Vector3 ToWorldPos(float z = 0.0f)
    {
        return new Vector3(WorldX, WorldY, 0.0f);
    }

    public Vector2 ToWorldPos2D()
    {
        return new Vector2(WorldX, WorldY);
    }

    public Vector2 ToGridPos()
    {
        return new Vector2(GridX, GridY);
    }

    public override string ToString()
    {
        return $"({GridX},{GridY})";
    }
}

public class TileClickedEventArgs : EventArgs
{
    public MouseButton Button;
    public MouseEvent MouseEvent;
    public GridTile Tile;
}

public class TilemapManager : MonoBehaviour
{
    public Tilemap Tilemap;
    public Tilemap[] PropsTilemaps;

    private BFSPathfinding _pathfinding = new BFSPathfinding();

    private Bounds _worldBounds;

    public event EventHandler<TileClickedEventArgs> TileClicked;

    void Start()
    {
        UpdateWorldBounds();
    }

    void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        if (leftClick || rightClick) {
            var point = Utils.MouseToWorldPoint();
            var tile = GetGridTileAtWorldPos(point.x, point.y);
            if (tile != null && TileClicked != null)
            {
                TileClicked.Invoke(this, new TileClickedEventArgs()
                {
                    Button = leftClick ? MouseButton.LeftButton : MouseButton.RightButton,
                    MouseEvent = MouseEvent.MouseDown,
                    Tile = tile,
                });
            }
        }
    }

    public TravelPath GetPath(GridTile start, GridTile end)
    {
        return GetPath(new Vector3Int(start.GridX, start.GridY, 0), new Vector3Int(end.GridX, end.GridY, 0));
    }

    public TravelPath GetPath(Vector3Int start, Vector3Int end)
    {
        return this._pathfinding.GetPath(this, start, end);
    }
    public GridTile GetGridTileAtWorldPos(Vector3 pos)
    {
        return GetGridTileAtWorldPos(pos.x, pos.y);
    }

    public GridTile GetGridTileAtWorldPos(float x, float y)
    {
        var cell = Tilemap.WorldToCell(new Vector3(x, y, 0));
        return GetGridTile(cell.x, cell.y);
    }

    public GridTile GetGridTile(int x, int y)
    {
        var tile = Tilemap.GetTile<WorldTile>(new Vector3Int(x, y, 0));
        if (tile == null)
        {
            return null;
        }

        var data = tile.TileData;
        foreach (var propMap in PropsTilemaps)
        {
            // Get all props data
            var propsTile = propMap.GetTile<PropsTile>(new Vector3Int(x, y, 0));
            if (propsTile != null)
            {
                data = data.Combine(propsTile.TileData);
            }
        }

        Vector3 worldLoc = Tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
        return new GridTile()
        {
            Data = data,
            GridX = x,
            GridY = y,
            WorldX = worldLoc.x,
            //WorldY = worldLoc.y + 0.25f, // 0.25f is the vertical offset to the tile center
            WorldY = worldLoc.y,
        };
    }

    public List<GridTile> GetNeighborTileData(int x, int y)
    {
        var items = new List<GridTile>();
        var neighbors = new List<GridTile>();
        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                if (!(j == 0 && i == 0))
                {
                    neighbors.Add(GetGridTile(x + i, y + j));
                }
            }
        }

        foreach (var tile in neighbors)
        {
            if (tile != null)
            {
                items.Add(tile);
            }
        }

        return items;
    }

    private void UpdateWorldBounds()
    {
        Tilemap.CompressBounds();
        _worldBounds = Tilemap.GetComponent<TilemapRenderer>().bounds;

        var follow = Camera.main.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.SetBounds(_worldBounds);
        }
    }
}
