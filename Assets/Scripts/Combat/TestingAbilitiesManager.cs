using System;
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
}

public class TestingAbilitiesManager : MonoBehaviour
{
    public bool ArmyMode = false;

    [HideIf("ArmyMode")]
    public GameObject Left;
    
    [HideIf("ArmyMode")]
    public GameObject Right;

    [HideIf("ArmyMode")]
    public CombatAbilityData AbilityData;

    [ShowIf("ArmyMode")]
    public TestArmyConfiguration LeftArmyConfig;

    [ShowIf("ArmyMode")]
    public TestArmyConfiguration RightArmyConfig;

    private TestArmy _leftArmy;

    private TestArmy _rightArmy;

    public BattleManager BattleManager;

    // Start is called before the first frame update
    void Start()
    {
        if (ArmyMode) {
            var enemyFaction = ResourceHelper.Factions.First(a => !a.IsPlayerFaction).Faction;
            var playerFaction = ResourceHelper.Factions.First(a => a.IsPlayerFaction).Faction;

            _leftArmy = MakeArmy(LeftArmyConfig, playerFaction);
            _rightArmy = MakeArmy(RightArmyConfig, enemyFaction);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private TestArmy MakeArmy(TestArmyConfiguration config, Faction faction)
    {
        var army = new TestArmy();
        var lvl = config.Level;
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.FL, faction, lvl), FormationUtils.FL);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.FM, faction, lvl), FormationUtils.FM);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.FR, faction, lvl), FormationUtils.FR);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.ML, faction, lvl), FormationUtils.ML);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.MM, faction, lvl), FormationUtils.MM);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.MR, faction, lvl), FormationUtils.MR);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.BL, faction, lvl), FormationUtils.BL);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.BM, faction, lvl), FormationUtils.BM);
        army.Formation.PutUnit(UnitGenerator.MakeUnit(config.BR, faction, lvl), FormationUtils.BR);
        return army;
    }

    public void DoAbility()
    {
        if (!ArmyMode)
        {
            var obj = new GameObject("Ability");
            var ability = obj.AddComponent<CombatAbility>();

            ability.InitData(AbilityData);
            ability.StartTargetedAbility(Left, Right);
        }
        else
        {
            BattleManager.StartCombat(_leftArmy, _rightArmy);
        }
    }
}
