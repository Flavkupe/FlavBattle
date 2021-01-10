using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using FlavBattle.Combat;

public class ArmyEnteredGarrisonEventArgs : EventArgs
{
    public Army Initiator;
    public Garrison Garrison;
}

public class ArmyLeftGarrisonEventArgs : EventArgs
{
    public Army Initiator;
    public Garrison Garrison;
}

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
    private GarrisonManager _garrisonManager;
    private GameEventManager _gameEvents;
    private BattleManager _battleManager;
    private CameraMain _camera;

    /// <summary>
    /// Event indicating that the Army has reached a specific garrison
    /// </summary>
    public event EventHandler<ArmyEnteredGarrisonEventArgs> ArmyEnteredGarrison;

    public event EventHandler<ArmyLeftGarrisonEventArgs> ArmyLeftGarrison;

    public bool ArmyIsSelected => _selected != null;

    void Awake()
    {
        _ui = FindObjectOfType<UIManager>();
        Debug.Assert(_ui != null, "UIManager not found");

        _gameEvents = FindObjectOfType<GameEventManager>();
        _battleManager = FindObjectOfType<BattleManager>();
        _camera = FindObjectOfType<CameraMain>();
        _garrisonManager = FindObjectOfType<GarrisonManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        TileMap.TileClicked += OnMapTileClicked;

        var enemyFaction = ResourceHelper.Factions.First(a => !a.IsPlayerFaction);
        _playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction);

        // TODO: TEMP
        // var playerArmy = CreateArmyWithUnits(0, 0, _playerFaction, 4, 4);
        // var playerArmy2 = CreateArmyWithUnits(0, -2, _playerFaction, 3, 4);

        var spawners = FindObjectsOfType<ArmyMapSpawn>();
        foreach (var spawner in spawners)
        {
            if (spawner.SpawnOnStart)
            {
                CreateArmyFromSpawner(spawner);
            }

            spawner.SpawnTriggered += HandleSpawnTriggered;
        }

        // var enemyArmy = CreateArmyWithUnits(-2, -3, enemyFaction, 3, 1);

        _ui.ArmyPanel.UpdatePanelContents();
        _ui.ArmyPanel.ArmyClicked += HandleArmyClickedFromPanel;
        _ui.ArmyPanel.ArmyEditRequested += OnEditArmy;
        _gameEvents.CombatEndedEvent += HandleCombatEndedEvent;
    }

    private void HandleSpawnTriggered(object sender, ArmyMapSpawn e)
    {
        CreateArmyFromSpawner(e);
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

    public Army CreateArmy(int gridX, int gridY, FactionData faction)
    {
        var startTile = TileMap.GetGridTile(gridX, gridY);
        var army = Instantiate(ArmyTemplate);
        army.SetFaction(faction);
        InitArmy(army);
        army.PutOnTile(startTile);
        return army;
    }

    public Army CreateArmyWithUnits(int gridX, int gridY, FactionData faction, int numUnits, int unitLevel)
    {
        var army = CreateArmy(gridX, gridY, faction);

        UnitGenerator.PopulateArmy(army, _playerFaction.Faction, new UnitGenerator.RandomArmyOptions
        {
            MinLevel = unitLevel,
            MaxLevel = unitLevel,
            MinUnitNum = numUnits,
            MaxUnitNum = numUnits,
        });

        return army;
    }

    public Army CreateArmy(IArmy army)
    {
        var newArmy = Instantiate(ArmyTemplate);
        newArmy.CopyFrom(army);
        InitArmy(newArmy);
        return newArmy;
    }

    private Army CreateArmyFromSpawner(ArmyMapSpawn spawn)
    {
        var army = spawn.SpawnArmy();
        var startTile = TileMap.GetGridTileAtWorldPos(army.transform.position);
        InitArmy(army);
        army.PutOnTile(startTile);
        Destroy(spawn.gameObject);
        return army;       
    }

    private void InitArmy(Army army)
    {
        army.SetMap(TileMap);
        army.ArmyClicked += HandleArmyClicked;
        army.ArmyEncountered += HandleArmyEncountered;
        army.EnterTile += HandleArmyEnterTile;
        army.ExitTile += HandleArmyExitTile;
        _armies.Add(army);

        // Update UI
        _ui.ArmyCreated(army);
    }

    public void GarrisonSelectedArmy()
    {
        if (_selected != null)
        {
            var army = _selected;
            this.UnselectAll();
            _garrisonManager.GarrisonArmy(army);
            Destroy(army.gameObject);
            _armies.Remove(army);
        }
    }

    private void HandleArmyExitTile(object sender, ExitTileEventArgs e)
    {
        Utils.LogTrace("Army exited tile", this);
        var garrison = e.Tile.GetComponent<Garrison>();
        if (garrison != null)
        {
            ArmyLeftGarrison?.Invoke(this, new ArmyLeftGarrisonEventArgs()
            {
                Initiator = e.Initiator,
                Garrison = garrison
            });
        }
    }

    private void HandleArmyEnterTile(object sender, EnterTileEventArgs e)
    {
        Utils.LogTrace("Army entered tile", this);
        var garrison = e.Tile.GetComponent<Garrison>();
        if (garrison != null)
        {
            ArmyEnteredGarrison?.Invoke(this, new ArmyEnteredGarrisonEventArgs()
            {
                Initiator = e.Initiator,
                Garrison = garrison
            });
        }
    }

    private void OnMapTileClicked(object sender, TileClickedEventArgs e)
    {
        if (_selected != null &&
            e.MouseEvent == MouseEvent.MouseDown)
        {
            if (e.Button == MouseButton.RightButton)
            {
                if (!_selected.IsCommandable)
                {
                    // Do not move uncommandable units (fleeing, etc)
                    return;
                }

                var start = TileMap.GetGridTileAtWorldPos(_selected.transform.position);
                var path = TileMap.GetPath(start, e.Tile);
                _selected.SetPath(path);
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

        Sounds.Play(CommonSoundType.Select);

        UpdateSelectedArmyUI(army);

        if (IsPlayerArmy(_selected))
        {
            _selected.Select();
        }
        else
        {
            _selected = null;
        }
    }

    private void HandleArmyClicked(object sender, ArmyClickedEventArgs args)
    {
        if (args.Button == MouseButton.LeftButton)
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
        else if (args.Button == MouseButton.RightButton)
        {
            if (this.ArmyIsSelected && args.Clicked != _selected)
            {
                // Do not open window if an army is selected (unless it's this army)
                return;
            }

            if (args.Clicked != null)
            {
                _ui.ShowArmyEditWindow(args.Clicked);
            }
        }
    }

    private void HandleArmyEncountered(object sender, ArmyEncounteredEventArgs e)
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
        StartCoroutine(CombatEnded(e.Winner, e.Loser, e.VictoryType));
    }

    private IEnumerator CombatEnded(IArmy winner, IArmy loser, VictoryType victoryType)
    {
        var loserArmy = _armies.FirstOrDefault(a => a.ID == loser.ID);
        Debug.Assert(loserArmy != null);
        if (loserArmy != null)
        {
            if (victoryType == VictoryType.Destroyed)
            {
                // Army vanquished
                _armies.Remove(loserArmy);
                yield return loserArmy.Vanish();
                Destroy(loserArmy.gameObject);
            }
            else if (victoryType == VictoryType.Fled)
            {
                // set fleeing state
                FleeFromOther(loser, winner);
            }

            _combatProcessed = false;
            PauseAll(false);
        }
    }

    /// <summary>
    /// Causes army 'army' to flee from 'other' by going to the furthest valid square away from
    /// the other army, within a 1 block space.
    /// </summary>
    /// <returns></returns>
    private void FleeFromOther(IArmy army, IArmy other)
    {
        var armyObj = GetArmyByID(army.ID);
        var otherObj = GetArmyByID(other.ID);
        var currTile = TileMap.GetGridTileAtWorldPos(armyObj.transform.position);
        var enemyTile = TileMap.GetGridTileAtWorldPos(otherObj.transform.position);
        var tiles = TileMap.GetNeighborTileData(currTile.GridX, currTile.GridY);

        // Filter by only passable tiles
        tiles = tiles.Where(a => a.Data.Passable).ToList();

        // Get the tile with the highest distance from the other unit.
        var furthest = tiles.GetMax(a => Vector2.Distance(enemyTile.ToGridPos(), a.ToGridPos()));
        var path = TileMap.GetPath(currTile, furthest);
        armyObj.SetPath(path);
        armyObj.SetFleeing(true);        
    }

    /// <summary>
    /// Gets an army from the interface by comparing IDs
    /// </summary>
    /// <param name="army"></param>
    /// <returns></returns>
    private Army GetArmyByID(string armyID)
    {
        return this._armies.First(a => a.ID == armyID);
    }

    private IEnumerator StartCombat(Army player, Army enemy)
    {
        PauseAll(true);
        yield return _camera.PanTo(player.transform.position);
        var middle = (player.transform.position + enemy.transform.position) / 2;
        middle.y += 0.25f;
        var icon = Instantiate(BattleIndicator);
        icon.transform.position = middle;
        yield return icon.SpinAround();
        _battleManager.StartCombat(player, enemy);
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

        if (pause)
        {
            _gameEvents.TriggerMapEvent(MapEventType.MapPaused);
        } 
        else
        {
            _gameEvents.TriggerMapEvent(MapEventType.MapUnpaused);
        }
    }

    private void UnselectAll()
    {
        _selected = null;
        foreach (var army in _armies)
        {
            army.Unselect();
        }

        UpdateSelectedArmyUI(null);
    }

    private void UpdateSelectedArmyUI(Army army)
    {
        _ui.UpdateSelectedArmyUI(army);
    }

    private void HandleArmyClickedFromPanel(object source, IArmy clickedArmy)
    {
        var army = _armies.First(a => a.ID == clickedArmy.ID);
        Camera.main.transform.position = army.transform.position.SetZ(Camera.main.transform.position.z);
        SelectArmy(army);
    }
}
