using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GarrisonManager : MonoBehaviour
{
    public bool IsCombatMap = true;

    private List<StoredArmy> _garrisonedArmies = new List<StoredArmy>();

    private FactionData _playerFaction;

    private ArmyManager _armyManager;

    private UIManager _ui;

    private void Awake()
    {
        if (IsCombatMap)
        {
            // can deploy armies and such
            _armyManager = FindObjectOfType<ArmyManager>();
        }

        _ui = FindObjectOfType<UIManager>();
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
    }

    public void EditGarrison()
    {
        _ui.ShowGarrisonWindow(_garrisonedArmies.ToArray());
    }
}
