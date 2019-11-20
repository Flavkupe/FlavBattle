using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        var tile = tilemap.GetTile<WorldTile>(new Vector3Int(0, 0, 0));
        tile.DoShit();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
