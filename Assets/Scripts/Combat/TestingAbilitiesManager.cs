﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class TestArmyConfiguration
{
    public int Level = 1;

    public UnitData FL;
    public UnitData FM;
    public UnitData FR;

    public UnitData ML;
    public UnitData MM;
    public UnitData MR;

    public UnitData BL;
    public UnitData BM;
    public UnitData BR;

    public FormationRowAndCol OfficerPosition;

    public UnitData GetUnit(FormationRowAndCol rowAndCol)
    {
        switch (rowAndCol)
        {
            case FormationRowAndCol.FL:
                return FL;
            case FormationRowAndCol.FM:
                return FM;
            case FormationRowAndCol.FR:
                return FR;
            case FormationRowAndCol.ML:
                return ML;
            case FormationRowAndCol.MM:
                return MM;
            case FormationRowAndCol.MR:
                return MR;
            case FormationRowAndCol.BL:
                return BL;
            case FormationRowAndCol.BM:
                return BM;
            case FormationRowAndCol.BR:
            default:
                return BR;
        }
    }
}

public class TestingAbilitiesManager : MonoBehaviour
{
    public bool ArmyMode = false;

    /// <summary>
    /// Change to raise or lower the speed the game goes
    /// </summary>
    public float TimeScale = 1.0f;

    [HideIf("ArmyMode")]
    public GameObject Left;
    
    [HideIf("ArmyMode")]
    public GameObject Right;

    [HideIf("ArmyMode")]
    public CombatAbilityData AbilityData;

    [HideIf("ArmyMode")]
    public bool RightToLeft;

    [ShowIf("ArmyMode")]
    public TestArmyConfiguration LeftArmyConfig;

    [ShowIf("ArmyMode")]
    public TestArmyConfiguration RightArmyConfig;


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
            var enemyFaction = ResourceHelper.Factions.First(a => !a.IsPlayerFaction).Faction;
            var playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction).Faction;

            _leftArmy = MakeArmy(LeftArmyConfig, playerFaction);
            _rightArmy = MakeArmy(RightArmyConfig, enemyFaction);
        }
    }

    private TestArmy MakeArmy(TestArmyConfiguration config, Faction faction)
    {
        var army = new TestArmy();
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
                var newUnit = UnitGenerator.MakeUnit(unit, faction, lvl, rowAndCol == config.OfficerPosition);
                army.Formation.PutUnit(newUnit, FormationUtils.GetPair(rowAndCol));
            }
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
