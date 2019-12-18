using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    public TilemapManager TileMap;

    public Army ArmyTemplate;
    public AnimatedSpin BattleIndicator;

    private List<Army> _armies = new List<Army>();

    private bool _pauseAll = false;

    private Army _selected = null;

    private bool _clickProcessed = false;
    private bool _combatProcessed = false;

    private FactionData _playerFaction;
    private UIManager _ui;
    private GameEventManager _gameEvents;
    private BattleManager _battleManager;
    private CameraMain _camera;

    

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        _ui = FindObjectOfType<UIManager>();
        Debug.Assert(_ui != null, "UIManager not found");

        _gameEvents = FindObjectOfType<GameEventManager>();
        _battleManager = FindObjectOfType<BattleManager>();
        _camera = FindObjectOfType<CameraMain>();

        var enemyFaction = ResourceHelper.Factions.First(a => !a.IsPlayerFaction);
        _playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction);
        var playerArmy = CreateArmy(0, 0, _playerFaction);
        var playerArmy2 = CreateArmy(0, -2, _playerFaction);
        var enemyArmy = CreateArmy(-2, 0, enemyFaction);

        // TODO: TEMP
        playerArmy.Formation.PutUnit(UnitGenerator.MakeUnit(null, _playerFaction.Faction, 4));
        playerArmy.Formation.PutUnit(UnitGenerator.MakeUnit(null, _playerFaction.Faction, 4));
        playerArmy2.Formation.PutUnit(UnitGenerator.MakeUnit(null, _playerFaction.Faction, 4));
        playerArmy2.Formation.PutUnit(UnitGenerator.MakeUnit(null, _playerFaction.Faction, 4));
        enemyArmy.Formation.PutUnit(UnitGenerator.MakeUnit(null, enemyFaction.Faction));
        enemyArmy.Formation.PutUnit(UnitGenerator.MakeUnit(null, enemyFaction.Faction));

        _ui.ArmyPanel.UpdatePanelContents();
        _ui.ArmyPanel.ArmyClicked += HandleArmyClickedFromPanel;
        _ui.ArmyPanel.ArmyEditRequested += OnEditArmy;
        _gameEvents.CombatEndedEvent += HandleCombatEndedEvent;        
    }

    private void OnEditArmy(object sender, IArmy army)
    {
        _ui.ShowArmyEditWindow(army);
    }

    // Update is called once per frame
    void Update()
    {
        _clickProcessed = false;
    }

    public IEnumerable<Army> GetPlayerArmies()
    {
        return this._armies.Where(a => a.IsPlayerArmy);
    }

    public Army CreateArmy(int x, int y, FactionData faction)
    {
        var startTile = TileMap.GetGridTile(x, y);
        var army = Instantiate(ArmyTemplate);
        army.SetMap(TileMap);
        army.SetFaction(faction, faction == _playerFaction);
        army.PutOnTile(startTile);
        army.ArmyClicked += OnArmyClicked;
        army.ArmyEncountered += OnArmyEncountered;
        _armies.Add(army);

        // Update UI
        _ui.ArmyCreated(army);

        return army;
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

    private void SelectArmy(Army army)
    {
        UnselectAll();
        _selected = army;
        _ui.FormationPanel.Show();
        _ui.FormationPanel.SetArmy(army);

        if (IsPlayerArmy(_selected))
        {
            _selected.Select();
        }
        else
        {
            _selected = null;
        }
    }

    private void OnArmyClicked(object sender, ArmyClickedEventArgs args)
    {
        if (args.Clicked != _selected)
        {
            _clickProcessed = true;
            SelectArmy(args.Clicked);
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

        if (e.Initiator.Faction == e.Opponent.Faction)
        {
            return;
        }

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
        return army.Faction.Faction == this._playerFaction.Faction;
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

    private void HandleArmyClickedFromPanel(object source, IArmy clickedArmy)
    {
        var army = _armies.First(a => a.ID == clickedArmy.ID);
        Camera.main.transform.position = army.transform.position.SetZ(Camera.main.transform.position.z);
        SelectArmy(army);
    }
}
