using System;
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
}

public class TileClickedEventArgs : EventArgs
{
    public MouseButton Button;
    public MouseEvent MouseEvent;
    public GridTile Tile;
}

public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    private BFSPathfinding _pathfinding = new BFSPathfinding();

    public event EventHandler<TileClickedEventArgs> TileClicked;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        var cell = tilemap.WorldToCell(new Vector3(x, y, 0));
        return GetGridTile(cell.x, cell.y);
    }

    public GridTile GetGridTile(int x, int y)
    {
        var tile = tilemap.GetTile<WorldTile>(new Vector3Int(x, y, 0));
        if (tile == null)
        {
            return null;
        }

        Vector3 worldLoc = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
        return new GridTile()
        {
            Data = tile.TileData,
            GridX = x,
            GridY = y,
            WorldX = worldLoc.x,
            WorldY = worldLoc.y + 0.25f, // 0.25f is the vertical offset to the tile center
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
}
