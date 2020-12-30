using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BattleStateBase : IBattleState
{
    public abstract bool ShouldUpdate(BattleStatus state);
    protected abstract IEnumerator Run(BattleStatus state);

    protected MonoBehaviour _owner;

    public BattleStateBase(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void Update(BattleStatus state)
    {
        if (state.TurnExecuting)
        {
            return;
        }

        state.TurnExecuting = true;
        _owner.StartCoroutine(Execute(state));
    }

    private IEnumerator Execute(BattleStatus state)
    {
        yield return Run(state);
        state.TurnExecuting = false;
    }

    protected List<Combatant> PickTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
    {
        var empty = new List<Combatant>();
        if (target.TargetType == CombatAbilityTarget.Self)
        {
            return new List<Combatant>() { combatant };
        }
        
        // TODO: pick best targets based on other things
        //var targetPositions = targetArmy.Formation.GetOccupiedPositions(true);
        //var valid = FormationUtils.GetFormationPairs(ability.ValidTargets);
        //var targets = FormationUtils.GetIntersection(valid, targetPositions);
        var units = GetValidAbilityTargets(state, combatant, target);
        if (units.Count == 0)
        {
            return empty;
        }

        switch (target.TargetType)
        {
            case CombatAbilityTarget.Self:
                return new List<Combatant> { combatant };
            case CombatAbilityTarget.RandomEnemy:
            case CombatAbilityTarget.RandomAlly:
                return new List<Combatant> { units.GetRandom() };
            case CombatAbilityTarget.AllAllies:
            case CombatAbilityTarget.AllEnemies:
            default:
                return units;
        }
    }

    /// <summary>
    /// Gets the units that are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    protected List<Combatant> GetValidAbilityTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
    {
        var targetArmy = target.AffectsAllies() ? combatant.Allies : combatant.Enemies;
        var validPositions = FormationUtils.GetFormationPairs(target.ValidTargets);
        var validCombatants = state.GetCombatants(targetArmy.Formation.GetUnits(validPositions, true));
        if (target.ValidOpponent == ValidOpponent.Any)
        {
            return validCombatants;
        }

        if (target.ValidOpponent == ValidOpponent.LowerLevel)
        {
            return validCombatants.Where(a => a.Unit.Info.CurrentStats.Level < combatant.Unit.Info.CurrentStats.Level).ToList();
        }

        if (target.ValidOpponent == ValidOpponent.HigherLevel)
        {
            return validCombatants.Where(a => a.Unit.Info.CurrentStats.Level > combatant.Unit.Info.CurrentStats.Level).ToList(); ;
        }

        Debug.LogWarning($"No check configured for ability validity type {target.ValidOpponent}; treating as 'Any'");
        return validCombatants;
    }

    /// <summary>
    /// Hides all UI for combat (Buttons, backdrop, etc)
    /// </summary>
    protected IEnumerator HideCombatUI(BattleStatus state)
    {
        state.BattleUIPanel.Hide();
        yield return state.BattleDisplay.HideCombatScene();
    }
}
