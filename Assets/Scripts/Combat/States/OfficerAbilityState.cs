using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OfficerAbilityState : BattleStateBase
{
    public OfficerAbilityState(MonoBehaviour owner) : base(owner)
    {
    }

    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.CombatPhase && state.AbilityQueue.Count > 0;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        yield return DoQueuedOfficerAbility(state);
    }

    /// <summary>
    /// Dequeues an officer ability from the list and executes it
    /// </summary>
    private IEnumerator DoQueuedOfficerAbility(BattleStatus state)
    {
        var officer = state.GetPlayerOfficer();
        var ability = state.AbilityQueue.Dequeue();
        yield return state.BattleUIPanel.AnimateAbilityNameCallout(ability);
        yield return DoOfficerAbility(state, officer, ability);
    }


    /// <summary>
    /// Performs an officer ability based on OfficerAbilityData, such as when clicking on an action or
    /// due to events like combat start.
    /// </summary>
    private IEnumerator DoOfficerAbility(BattleStatus state, Combatant combatant, OfficerAbilityData officerAbility)
    {
        var target = officerAbility.Target;
        var targets = PickTargets(state, combatant, target);
        var ability = officerAbility.CombatAbility;

        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name} is doing officer action {ability.Name}!");

        // TODO: other multipliers
        var multiplier = officerAbility.MultiplierType ==
            OfficerAbilityEffectMultiplierType.Constant ? officerAbility.ConstantEffectMultiplier : 1.0f;
        yield return UseAbility(state, combatant, ability, targets, multiplier);
    }
}

