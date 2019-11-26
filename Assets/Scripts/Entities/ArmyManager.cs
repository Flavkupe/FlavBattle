using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    public TilemapManager TileMap;

    public Army ArmyTemplate;

    private UnitData[] _allUnitDataResources;
    private FactionData[] _allFactionDataResources;

    private List<Army> _armies = new List<Army>();

    private Army _selected = null;

    private bool _clickProcessed = false;

    private UIManager _ui;

    public FactionData PlayerFaction;

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        _ui = FindObjectOfType<UIManager>();
        Debug.Assert(_ui != null, "UIManager not found");

        _allUnitDataResources = Utils.LoadAssets<UnitData>("Units");
        _allFactionDataResources = Utils.LoadAssets<FactionData>("Factions");

        var enemyFaction = _allFactionDataResources.First(a => a.Faction != PlayerFaction.Faction);
        CreateArmy(0, 0, PlayerFaction);
        CreateArmy(-2, 0, enemyFaction);
    }

    // Update is called once per frame
    void Update()
    {
        _clickProcessed = false;
    }

    public void CreateArmy(int x, int y, FactionData faction)
    {
        var startTile = TileMap.GetGridTile(x, y);
        var army = Instantiate(ArmyTemplate);
        army.SetMap(TileMap);
        army.SetFaction(faction);
        army.PutOnTile(startTile);
        army.ArmyClicked += OnArmyClicked;
        var unit1 = this.MakeUnit(null);
        var unit2 = this.MakeUnit(null);

        army.Formation.PutUnit(unit1);
        army.Formation.PutUnit(unit2);
        _armies.Add(army);
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
                if (!_clickProcessed)
                {
                    UnselectAll();
                }
            }
        }
        else if (!_clickProcessed)
        {
            UnselectAll();
        }
    }

    private void OnArmyClicked(object sender, ArmyClickedEventArgs args)
    {
        if (args.Clicked != _selected)
        {
            UnselectAll();
            _selected = args.Clicked;
            _clickProcessed = true;
            _ui.FormationPanel.Show();
            _ui.FormationPanel.SetFormation(_selected.Formation);

            if (IsPlayerArmy(_selected))
            {
                _selected.Select();
            }
            else
            {
                _selected = null;
            }
        }
        else
        {
            UnselectAll();
        }
    }

    private bool IsPlayerArmy(Army army)
    {
        return army.Faction.Faction == this.PlayerFaction.Faction;
    }

    private void UnselectAll()
    {
        _selected = null;
        foreach (var army in _armies)
        {
            army.Unselect();
        }

        _ui.FormationPanel.Hide();
    }
}
