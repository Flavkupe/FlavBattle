using FlavBattle.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GarrisonManager : MonoBehaviour
{
    public bool IsCombatMap = true;

    private List<StoredArmy> _garrisonedArmies = new List<StoredArmy>();

    private List<Unit> _garrisonedUnits = new List<Unit>();

    private List<Army> _armiesOnGarrisonTile = new List<Army>();

    private FactionData _playerFaction;

    private ArmyManager _armyManager;

    private UIManager _ui;

    private GameEventManager _gameEvents;

    public Garrison _garrison;

    private void Awake()
    {
        if (IsCombatMap)
        {
            // can deploy armies and such
            _armyManager = FindObjectOfType<ArmyManager>();
        }

        _ui = FindObjectOfType<UIManager>();
        _ui.UnitReplaced += HandleUnitReplaced;
        _ui.ArmyDeployed += HandleArmyDeployed;

        _gameEvents = FindObjectOfType<GameEventManager>();
        _gameEvents.UnitDeployed += HandleUnitDeployed;
        _gameEvents.UnitGarrisoned += HandleUnitGarrisoned;

        _garrison = FindObjectOfType<Garrison>();
        _garrison.RightClicked += HandleGarrisonRightClicked;
        Debug.Assert(_garrison != null, "No garrison found!");
    }

    private void HandleGarrisonRightClicked(object sender, EventArgs e)
    {
        if (_armyManager.ArmyIsSelected)
        {
            // Ignore right click if army is selected
            return;
        }

        this.EditGarrison();
    }

    private void HandleArmyDeployed(object sender, IArmy army)
    {
        var stored = _garrisonedArmies.FirstOrDefault(a => a.ID == army.ID);
        Debug.Assert(stored != null, "Expected army not in garrison!");
        this.DeployArmy(stored);
    }

    private void HandleUnitGarrisoned(object sender, Unit e)
    {
        if (!_garrisonedUnits.Contains(e))
        {
            _garrisonedUnits.Add(e);
        }
    }

    private void HandleUnitDeployed(object sender, Unit e)
    {
        if (_garrisonedUnits.Contains(e))
        {
            _garrisonedUnits.Remove(e);
        }
    }

    private void HandleUnitReplaced(object sender, Unit e)
    {
        _garrisonedUnits.Add(e);
    }

    private void Start()
    {
        _playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction);

        // TEMP: generate some starting armies
        for (var i = 0; i < 8; i++)
        {
            var army1 = new StoredArmy(_playerFaction);

            UnitGenerator.PopulateArmy(army1, _playerFaction.Faction, new UnitGenerator.RandomArmyOptions {
                MinLevel = 2,
                MaxLevel = 4,
                MinUnitNum = 3,
                MaxUnitNum = 5,
            });

            _garrisonedArmies.Add(army1);
        }

        for (var i = 0; i < 4; i++)
        {
            var rand = UnityEngine.Random.Range(1, 5);
            var unit = UnitGenerator.MakeUnit(_playerFaction.Faction, rand);
            _garrisonedUnits.Add(unit);
        }
    }

    public void EditGarrison()
    {
        _gameEvents.TriggerMapEvent(MapEventType.MapPaused);
        _ui.ShowGarrisonWindow(_garrisonedArmies.ToArray(), _garrisonedUnits.ToArray());
    }

    /// <summary>
    /// Adds the army to the garrison, creating a record of it.
    /// Does NOT destroy the Army object.
    /// </summary>
    public void GarrisonArmy(Army army)
    {
        _garrisonedArmies.Add(new StoredArmy(army));
    }

    public void DeployArmy(StoredArmy army)
    {
        var pos = _garrison.transform.position;
        var newArmy = _armyManager.CreateArmy(army);
        newArmy.transform.position = pos.ShiftY(-0.25f);
        _garrisonedArmies.Remove(army);
        _ui.UpdateGarrisonWindow(_garrisonedArmies.ToArray(), _garrisonedUnits.ToArray());
    }
}
