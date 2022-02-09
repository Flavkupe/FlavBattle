using System;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using FlavBattle.Combat;
using FlavBattle.Combat.Animation;
using FlavBattle.Entities.Data;

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
    public UnitData LeftChar;

    [HideIf("ArmyMode")]
    public UnitData RightChar;

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

    public CombatTurnActionSummary[] Summary;

    public CombatFullTurnAnimationData Animation;

    public CombatAnimationGraph Graph;

    // Start is called before the first frame update
    void Start()
    {
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
            //var obj = new GameObject("Ability");
            //var ability = obj.AddComponent<CombatAbility>();

            //ability.InitData(AbilityData);
            //if (RightToLeft)
            //{
            //    ability.StartTargetedAbility(Right, Left);
            //}
            //else
            //{
            //    ability.StartTargetedAbility(Left, Right);
            //}

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

        var summary = new CombatTurnUnitSummary();
        summary.Results.AddRange(this.Summary);
        summary.Ability = summary.Results[0].Ability;
        summary.Source = RightToLeft ? rightCombatant : leftCombatant;

        foreach (var item in summary.Results)
        {
            if (RightToLeft)
            {
                item.Source = rightCombatant;
                item.Target = leftCombatant;
            }
            else
            {
                item.Source = leftCombatant;
                item.Target = rightCombatant;
            }
        }

        // var anim = Animation.Create(summary);
        var anim = Graph.GetAnimation(summary);
        StartCoroutine(anim.Do());
    }
}
