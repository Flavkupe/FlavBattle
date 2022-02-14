using System;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using FlavBattle.Combat;
using FlavBattle.Combat.Animation;
using FlavBattle.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using FlavBattle.Components;

public class TestingAbilitiesManager : MonoBehaviour
{
    private enum TargetType
    {
        SingleEnemy,
        EnemiesAll,
        EnemiesRoundRobin,
        AlliesAll,
        AlliesRoundRobin,
    }

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
    public UnitData LeftChar;

    [HideIf("ArmyMode")]
    public UnitData RightChar;

    [HideIf("ArmyMode")]
    public UnitData[] LeftAllies;

    [HideIf("ArmyMode")]
    public UnitData[] RightAllies;

    [HideIf("ArmyMode")]
    public CombatAbilityData AbilityData;

    [HideIf("ArmyMode")]
    public bool RightToLeft;

    [HideIf("ArmyMode")]
    [SerializeField]
    private TargetType _targetType;

    [ShowIf("ArmyMode")]
    public InitialFormation LeftArmyConfig;

    [ShowIf("ArmyMode")]
    public InitialFormation RightArmyConfig;

    private TestArmy _leftArmy;
    private TestArmy _rightArmy;

    private List<CombatUnit> _leftAlliesCombatUnits = new List<CombatUnit>();
    private List<CombatUnit> _rightAlliesCombatUnits = new List<CombatUnit>();

    public BattleManager BattleManager;

    public CombatTurnActionSummary[] Summary;

    public CombatAnimationGraph Graph;

    // Start is called before the first frame update
    void Start()
    {
        var shiftX = 0.0f;
        foreach (var item in LeftAllies)
        {
            shiftX -= 1.0f;
            var clone = GameObject.Instantiate(this.Left);
            clone.SetUnit(UnitGenerator.MakeUnit(item, Faction.KingsMen), false);
            clone.transform.position = clone.transform.position.ShiftX(shiftX);
            clone.name = item.ClassName;
            _leftAlliesCombatUnits.Add(clone);
        }

        var unitleft = UnitGenerator.MakeUnit(LeftChar, Faction.KingsMen);
        var unitright = UnitGenerator.MakeUnit(RightChar, Faction.KingsMen);
        this.Left.SetUnit(unitleft, false);
        this.Right.SetUnit(unitright, true);
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
            Left.Hide();
            Right.Hide();
            foreach (var unit in _leftAlliesCombatUnits)
            {
                unit.Hide();
            }

            foreach (var unit in _rightAlliesCombatUnits)
            {
                unit.Hide();
            }

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
        if (!ArmyMode)
        {
            TestAbility();
        }
        else
        {
            InitArmy();
            BattleManager.StartCombat(_leftArmy, _rightArmy);
        }
    }

    [ContextMenu("TestAbility")]
    public void TestAbility()
    {
        var leftCombatant = new TestCombatant(Left);
        var rightCombatant = new TestCombatant(Right);

        var resultList = new List<CombatTurnActionSummary>(this.Summary);
        var summary = new CombatTurnUnitSummary();
        summary.Ability = AbilityData;
        summary.Source = RightToLeft ? rightCombatant : leftCombatant;

        if (_targetType == TargetType.SingleEnemy)
        {
            foreach (var item in resultList)
            {
                var copy = item.Clone();                
                if (RightToLeft)
                {
                    copy.Source = rightCombatant;
                    copy.Target = leftCombatant;
                }
                else
                {
                    copy.Source = leftCombatant;
                    copy.Target = rightCombatant;
                }

                summary.Results.Add(copy);
            }
        }
        else if (_targetType == TargetType.AlliesAll || _targetType == TargetType.EnemiesAll)
        {
            var units = _targetType == TargetType.AlliesAll ? _leftAlliesCombatUnits : _rightAlliesCombatUnits;
            foreach (var item in resultList)
            {
                foreach (var ally in units)
                {
                    var copy = item.Clone();
                    copy.Source = leftCombatant;
                    copy.Target = new TestCombatant(ally);
                    summary.Results.Add(copy);
                }

                // include main char
                if (_targetType == TargetType.AlliesAll)
                {
                    var selfCopy = item.Clone();
                    selfCopy.Source = leftCombatant;
                    selfCopy.Target = leftCombatant;
                    summary.Results.Add(selfCopy);
                }
            }
        }

        var anim = Graph.GetStartStep(summary);
        StartCoroutine(anim.Do());
    }
}
