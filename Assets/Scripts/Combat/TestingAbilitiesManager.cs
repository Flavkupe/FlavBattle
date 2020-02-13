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
