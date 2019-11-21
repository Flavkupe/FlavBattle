using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelPath
{
    public Queue<TileNode> Nodes = new Queue<TileNode>();
}

public class TileNode
{
    public TileNode(GridTile tile)
    {
        this.Tile = tile;
    }

    public GridTile Tile;
    public int Cost = 0;
    
    // Best known path to get to given cost
    public TileNode From;

    public override string ToString()
    {
        if (this.Tile == null)
        {
            return null;
        }

        return $"{this.Tile.GridX},{this.Tile.GridY}";
    }
}

public class BFSPathfinding
{
    private Dictionary<string, TileNode> _visited = new Dictionary<string, TileNode>();

    private Queue<TileNode> _toCheck = new Queue<TileNode>();

    public TravelPath GetPath(TilemapManager map, Vector3Int start, Vector3Int end)
    {
        _visited.Clear();

        var startTile = map.GetGridTile(start.x, start.y);
        var endTile = map.GetGridTile(end.x, end.y);
        var node = new TileNode(startTile);
        node.Cost = 0;
        _toCheck.Enqueue(node);

        while(_toCheck.Count > 0)
        {
            var next = _toCheck.Dequeue();
            this.VisitNode(map, next);
        }

        var finalNode = new TileNode(endTile);
        if (!WasVisited(finalNode)) {
            return null;
        }

        var final = _visited[finalNode.ToString()];

        var nodes = new List<TileNode>();
        nodes.Add(final);
        Debug.Log(final.ToString());
        var prev = final.From;
        while (prev != null)
        {
            Debug.Log(prev.ToString());
            
            nodes.Add(prev);
            prev = prev.From;
        }

        nodes.Reverse();
        var path = new TravelPath()
        {
            Nodes = new Queue<TileNode>(nodes),   
        };

        return path;
    }

    private void CheckNeighbors(TilemapManager map, TileNode node)
    {
        foreach (var neighbor in map.GetNeighborTileData(node.Tile.GridX, node.Tile.GridY))
        {
            var neighborNode = new TileNode(neighbor);
            neighborNode.From = node;
            neighborNode.Cost = node.Cost + neighborNode.Tile.Data.WalkCost;
            if (neighborNode.Tile.Data.Passable)
            {
                this._toCheck.Enqueue(neighborNode);
            }
        }
    }

    private void VisitNode(TilemapManager map, TileNode node)
    {
        var key = node.ToString();
        if (key == null)
        {
            return;
        }

        if (WasVisited(node))
        {
            if (_visited[key].Cost > node.Cost)
            {
                _visited[key] = node;
                CheckNeighbors(map, node);
            }
        }
        else
        {
            _visited[key] = node;
            CheckNeighbors(map, node);
        }
    }

    private bool WasVisited(TileNode node)
    {
        var key = node.ToString();
        return key != null && _visited.ContainsKey(key);
    }
}
