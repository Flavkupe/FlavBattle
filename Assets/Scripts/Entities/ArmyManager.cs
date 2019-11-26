using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    public TilemapManager TileMap;

    public Army DudeTemplate;

    private UnitData[] _allUnitDataResources;

    private List<Army> _armies = new List<Army>();

    private Army _selected = null;

    private bool _entityClicked = false;

    private FormationPanel _formationPanel;

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        _formationPanel = FindObjectOfType<FormationPanel>();
        Debug.Assert(_formationPanel != null, "FormationPanel not found");

        _formationPanel.gameObject.SetActive(false);

        _allUnitDataResources = Resources.LoadAll<UnitData>("Units");
        var assets = string.Join<UnitData>(", ", _allUnitDataResources);
        Debug.Log($"Loaded assets {assets}");

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

        var unit1 = this.MakeUnit(null);
        var unit2 = this.MakeUnit(null);

        dude.Formation.PutUnit(unit1);
        dude.Formation.PutUnit(unit2);
        _armies.Add(dude);
    }

    public Unit MakeUnit(UnitData data)
    {
        if (data == null)
        {
            data = _allUnitDataResources.GetRandom();
        }

        var unit = Utils.MakeOfType<Unit>(data.Name);
        unit.Data = data;
        unit.Info = new UnitInfo(data);
        return unit;
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
            _formationPanel.gameObject.SetActive(true);
            _formationPanel.SetFormation(_selected.Formation);
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

        _formationPanel.gameObject.SetActive(false);
    }
}
