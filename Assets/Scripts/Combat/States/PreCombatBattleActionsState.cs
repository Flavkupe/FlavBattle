using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PreCombatBattleActionsState : BattleStateBase
{
    public PreCombatBattleActionsState(MonoBehaviour owner) : base(owner)
    {
    }

    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.PreCombatStart;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        PrepareOfficerActions(state);
        yield return null;
        state.Stage = BattleStatus.BattleStage.CombatPhase;
    }

    private void PrepareOfficerActions(BattleStatus state)
    {
        // TODO: enemy army as well
        var officer = state.GetPlayerOfficer();
        EnqueueOfficerActions(state, officer);
    }

    private void EnqueueOfficerActions(BattleStatus state, Combatant combatant)
    {
        var actions = combatant.Unit.Info.OfficerAbilities.Where(a => a.TriggerType == OfficerAbilityTriggerType.AutoStartInCombat).ToList();
        if (actions.Count > 0)
        {
            // TODO: run each (in parallel...?) or pick a better one?
            var action = actions.GetRandom();
            state.AbilityQueue.Enqueue(action);
            // yield return DoOfficerAbility(state, combatant, action);
        }
        else
        {
            Debug.Log($"{combatant.Unit.Info.Faction}: officer {combatant.Unit.Info.Name} has no officer actions!");
        }
    }
}
