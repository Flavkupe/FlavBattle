using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    public TilemapManager TileMap;

    public Army ArmyTemplate;
    public AnimatedSpin BattleIndicator;

    private UnitData[] _allUnitDataResources;
    private FactionData[] _allFactionDataResources;

    private List<Army> _armies = new List<Army>();

    private bool _pauseAll = false;

    private Army _selected = null;

    private bool _clickProcessed = false;
    private bool _combatProcessed = false;

    private UIManager _ui;
    private GameEventManager _gameEvents;
    private BattleManager _battleManager;
    private CameraMain _camera;

    public FactionData PlayerFaction;

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        _ui = FindObjectOfType<UIManager>();
        Debug.Assert(_ui != null, "UIManager not found");

        _gameEvents = FindObjectOfType<GameEventManager>();
        _battleManager = FindObjectOfType<BattleManager>();
        _camera = FindObjectOfType<CameraMain>();

        _allUnitDataResources = Utils.LoadAssets<UnitData>("Units");
        _allFactionDataResources = Utils.LoadAssets<FactionData>("Factions");

        var enemyFaction = _allFactionDataResources.First(a => a.Faction != PlayerFaction.Faction);
        var playerArmy = CreateArmy(0, 0, PlayerFaction);
        var enemyArmy = CreateArmy(-2, 0, enemyFaction);

        // TODO: TEMP
        playerArmy.Formation.PutUnit(this.MakeUnit(null, PlayerFaction.Faction, 4));
        playerArmy.Formation.PutUnit(this.MakeUnit(null, PlayerFaction.Faction, 4));
        enemyArmy.Formation.PutUnit(this.MakeUnit(null, enemyFaction.Faction));
        enemyArmy.Formation.PutUnit(this.MakeUnit(null, enemyFaction.Faction));

        _gameEvents.CombatEndedEvent += HandleCombatEndedEvent;        
    }

    // Update is called once per frame
    void Update()
    {
        _clickProcessed = false;
    }

    public Army CreateArmy(int x, int y, FactionData faction)
    {
        var startTile = TileMap.GetGridTile(x, y);
        var army = Instantiate(ArmyTemplate);
        army.SetMap(TileMap);
        army.SetFaction(faction);
        army.PutOnTile(startTile);
        army.ArmyClicked += OnArmyClicked;
        army.ArmyEncountered += OnArmyEncountered;
        _armies.Add(army);
        return army;
    }

    public Unit MakeUnit(UnitData data, Faction faction, int level = 1)
    {
        if (data == null)
        {
            data = _allUnitDataResources.GetRandom();
        }

        var unit = new Unit()
        {
            Data = data,
            Info = new UnitInfo(data, faction, level)
        };

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

    private void OnArmyEncountered(object sender, ArmyEncounteredEventArgs e)
    {
        if (_combatProcessed)
        {
            return;
        }

        // TODO: combat end
        _combatProcessed = true;
        var player = IsPlayerArmy(e.Initiator) ? e.Initiator : e.Opponent;
        var other = IsPlayerArmy(e.Initiator) ? e.Opponent : e.Initiator;
        StartCoroutine(StartCombat(player, other));
    }

    private void HandleCombatEndedEvent(object sender, CombatEndedEventArgs e)
    {
        StartCoroutine(CombatEnded(e.Winner, e.Loser));
    }

    private IEnumerator CombatEnded(Army winner, Army loser)
    {
        _armies.Remove(loser);
        yield return loser.Vanish();
        Destroy(loser.gameObject);
        _combatProcessed = false;
        PauseAll(false);
    }

    private IEnumerator StartCombat(Army player, Army enemy)
    {
        PauseAll(true);
        yield return _camera.PanTo(player.transform.position);
        var middle = (player.transform.position + enemy.transform.position) / 2;
        middle.y += 0.25f;
        var icon = Instantiate(BattleIndicator);
        icon.transform.position = middle;
        _gameEvents.TriggerMapEvent(MapEventType.MapPaused);
        yield return icon.SpinAround();
        yield return _battleManager.StartCombat(player, enemy);
    }

    private bool IsPlayerArmy(Army army)
    {
        return army.Faction.Faction == this.PlayerFaction.Faction;
    }

    private void PauseAll(bool pause)
    {
        foreach (var army in _armies)
        {
            army.SetPaused(pause);
        }
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
