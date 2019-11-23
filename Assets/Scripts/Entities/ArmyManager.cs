using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    public TilemapManager TileMap;

    public Army DudeTemplate;

    private List<Army> _armies = new List<Army>();

    private Army _selected = null;

    private bool _entityClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        CreateArmy(0, 0);

        CreateArmy(-2, 0);
    }

    // Update is called once per frame
    void Update()
    {
        _entityClicked = false;
    }

    public void CreateArmy(int x, int y)
    {
        var startTile = TileMap.GetGridTile(x, y);
        var dude = Instantiate(DudeTemplate);
        dude.SetMap(TileMap);
        dude.PutOnTile(startTile);
        dude.ArmyClicked += OnArmyClicked;
        _armies.Add(dude);
    }

    private void OnMapTileClicked(object sender, TileClickedEventArgs e)
    {
        if (_selected != null &&
            e.MouseEvent == MouseEvent.MouseDown)
        {
            if (e.Button == MouseButton.RightButton)
            {
                var start = TileMap.GetGridTileAtWorldPos(_selected.transform.position);
                var path = TileMap.GetPath(start, e.Tile);
                _selected.SetPath(path);
                UnselectAll();
            }
            else if (e.Button == MouseButton.LeftButton)
            {
                if (!_entityClicked)
                {
                    UnselectAll();
                }
            }
        }
    }

    private void OnArmyClicked(object sender, ArmyClickedEventArgs args)
    {
        if (args.Clicked != _selected)
        {
            UnselectAll();
            _selected = args.Clicked;
            _selected.Select();
            _entityClicked = true;
        }
        else
        {
            UnselectAll();
        }
    }

    private void UnselectAll()
    {
        _selected = null;
        foreach (var army in _armies)
        {
            army.Unselect();
        }
    }
}
