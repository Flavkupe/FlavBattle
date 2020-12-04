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
        var strat = state.GetStrat(combatant);
        var decision = strat.Decide();

        var ability = decision.Ability;
        var targets = state.GetCombatants(decision.Targets);

        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name}'s turn!");

        // TODO: multiplier
        yield return UseAbility(state, combatant, ability, targets);
    }
}

