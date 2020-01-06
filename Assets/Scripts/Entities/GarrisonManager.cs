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

    private void Awake()
    {
        if (IsCombatMap)
        {
            // can deploy armies and such
            _armyManager = FindObjectOfType<ArmyManager>();
        }

        _ui = FindObjectOfType<UIManager>();
        _ui.UnitReplaced += HandleUnitReplaced;

        _gameEvents = FindObjectOfType<GameEventManager>();
        _gameEvents.UnitDeployed += HandleUnitDeployed;
        _gameEvents.UnitGarrisoned += HandleUnitGarrisoned;


        
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

        // TEMP
        for (var i = 0; i < 8; i++)
        {
            var army1 = new StoredArmy();            
            var rand = Random.Range(1, 5);
            for (var j = 0; j < rand; j++)
            {
                army1.Formation.PutUnit(UnitGenerator.MakeUnit(null, _playerFaction.Faction, rand));
            }

            _garrisonedArmies.Add(army1);
        }

        for (var i = 0; i < 4; i++)
        {
            var rand = Random.Range(1, 5);
            var unit = UnitGenerator.MakeUnit(null, _playerFaction.Faction, rand);
            _garrisonedUnits.Add(unit);
        }
    }

    public void EditGarrison()
    {
        _ui.ShowGarrisonWindow(_garrisonedArmies.ToArray(), _garrisonedUnits.ToArray());
    }
}
