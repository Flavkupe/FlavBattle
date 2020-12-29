using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NextCombatantTurnState : BattleStateBase
{
    public NextCombatantTurnState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are combatants in turn queue
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.CombatPhase && state.TurnQueue.Count > 0;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        var current = state.GetNextCombatant();
        if (current != null)
        {
            yield return DoTurn(state, current);
        }
    }

    private IEnumerator DoTurn(BattleStatus state, Combatant combatant)
    {
        var action = PickAction(state, combatant, combatant.Unit.Info.Actions);

        var targets = PickTargets(state, combatant, action.Target);

        // TODO: multiplier
        yield return UseAbility(state, combatant, action.Ability, targets);
    }

    /// <summary>
    /// Filters abilities by those that can target enemy formation
    /// </summary>
    private List<CombatAction> FilterPossibleActions(BattleStatus state, Combatant combatant, List<CombatAction> actions)
    {
        var enemyPositions = combatant.Enemies.Formation.GetOccupiedPositions(true);
        List<CombatAction> possible = new List<CombatAction>();
        foreach (var action in actions)
        {
            // Check if ability can hit any units
            if (CanHitUnits(state, combatant, action))
            {
                possible.Add(action);
            }
        }

        return possible;
    }

    /// <summary>
    /// Checks whether any units are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    private bool CanHitUnits(BattleStatus state, Combatant combatant, CombatAction ability)
    {
        return GetValidAbilityTargets(state, combatant, ability.Target).Count > 0;
    }

    /// <summary>
    /// Picks an attack by priority, if one exists. If not, then returns null
    /// (meaning there is no preference).
    /// </summary>
    private CombatAction PickAction(BattleStatus state, Combatant combatant, List<CombatAction> actions)
    {
        // First filter by possible attacks, and return default if none are possible.
        var possible = FilterPossibleActions(state, combatant, actions);

        if (possible.Count == 0)
        {
            Debug.Log("No valid actions! Returning global default");
            return GameResourceManager.Instance.GetDefaultCombatAction();
        }

        // Get random action from the top priority possible action
        var maxPriority = possible.Max(a => a.Priority);
        return possible.Where(a => a.Priority == maxPriority).ToList().GetRandom();
    }

    private List<CombatAbilityPriority> GetPriorityValuesReversed()
    {
        var list = new List<CombatAbilityPriority>();
        foreach (CombatAbilityPriority priority in Enum.GetValues(typeof(CombatAbilityPriority)))
        {
            list.Add(priority);
        }

        list.Reverse();
        return list;
    }
}

