using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using FlavBattle.Combat;

public class TestingAbilitiesManager : MonoBehaviour
{
    public bool ArmyMode = false;

    /// <summary>
    /// Change to raise or lower the speed the game goes
    /// </summary>
    public float TimeScale = 1.0f;

    [HideIf("ArmyMode")]
    public CombatUnit Left;
    
    [HideIf("ArmyMode")]
    public CombatUnit Right;

    [HideIf("ArmyMode")]
    public CombatAbilityData AbilityData;

    [HideIf("ArmyMode")]
    public bool RightToLeft;

    [ShowIf("ArmyMode")]
    public InitialFormation LeftArmyConfig;

    [ShowIf("ArmyMode")]
    public InitialFormation RightArmyConfig;


    public GameObject Thing;

    private TestArmy _leftArmy;

    private TestArmy _rightArmy;

    public BattleManager BattleManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = TimeScale;
    }

    private void InitArmy()
    {
        if (ArmyMode)
        {
            var enemyFaction = ResourceHelper.Factions.First(a => !a.IsPlayerFaction);
            var playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction);

            _leftArmy = MakeArmy(LeftArmyConfig, playerFaction);
            _rightArmy = MakeArmy(RightArmyConfig, enemyFaction);
        }
    }

    private TestArmy MakeArmy(InitialFormation config, FactionData factionData)
    {
        var army = new TestArmy();
        army.Faction = factionData;
        var lvl = config.Level;
        foreach (FormationRowAndCol rowAndCol in Enum.GetValues(typeof(FormationRowAndCol)))
        {
            var unit = config.GetUnit(rowAndCol);
            if (config.OfficerPosition == rowAndCol && unit == null)
            {
                Debug.LogError($"Specified {rowAndCol} for officer but no officer in slot!");
            }

            if (unit != null)
            {
                var newUnit = UnitGenerator.MakeUnit(unit, factionData.Faction, lvl, rowAndCol == config.OfficerPosition);
                army.Formation.PutUnit(newUnit, FormationUtils.GetPair(rowAndCol));
            }
        }

        if (army.Formation.GetOfficer() == null)
        {
            Debug.LogError("No officer for army!!");
        }

        return army;
    }

    public void DoAbility()
    {

        // TEMP
        //var positions = this.transform.GetComponentsInChildren<Transform>().Select(a => new Vector2(a.transform.position.x, a.transform.position.y)).ToArray();
        //var furthest = Utils.MathUtils.RandomFurthestPointAway(this.Thing.transform.position, positions, 2.0f, 10);
        //this.Thing.transform.position = furthest;

        if (!ArmyMode)
        {
            var obj = new GameObject("Ability");
            var ability = obj.AddComponent<CombatAbility>();

            ability.InitData(AbilityData);
            if (RightToLeft)
            {
                ability.StartTargetedAbility(Right, Left);
            }
            else
            {
                ability.StartTargetedAbility(Left, Right);
            }
            
        }
        else
        {
            InitArmy();
            BattleManager.StartCombat(_leftArmy, _rightArmy);
        }
    }
}
