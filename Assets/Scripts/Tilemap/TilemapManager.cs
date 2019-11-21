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
}

public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    public Army Dude;

    private BFSPathfinding _pathfinding = new BFSPathfinding();

    // Start is called before the first frame update
    void Start()
    {
        // var tile = tilemap.GetTile<WorldTile>(new Vector3Int(0, 0, 0));
        var loc = tilemap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
        var dude = Instantiate(Dude);
        dude.gameObject.transform.position = loc;

        // var loc2 = tilemap.GetCellCenterWorld(new Vector3Int(-1, -6, 0));
        // dude.SetDestination(loc2);

        var path = _pathfinding.GetPath(this, new Vector3Int(0, 0, 0), new Vector3Int(-1, -6, 0));
        dude.SetPath(path);
    }

    // Update is called once per frame
    void Update()
    {
        
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
            WorldY = worldLoc.y,
        };
    }

    public List<GridTile> GetNeighborTileData(int x, int y)
    {
        var items = new List<GridTile>();
        var neighbors = new List<GridTile>();
        neighbors.Add(GetGridTile(x - 1, y));
        neighbors.Add(GetGridTile(x + 1, y));
        neighbors.Add(GetGridTile(x, y + 1));
        neighbors.Add(GetGridTile(x, y - 1));
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
