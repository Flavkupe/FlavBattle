using FlavBattle.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InitRoundState : BattleStateBase
{
    public InitRoundState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are no combatants in turn queue during combat phase
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.CombatPhase && state.TurnQueue.Count == 0;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        yield return InitRound(state);
    }

    /// <summary>
    /// Initializes the round and queues up turns
    /// </summary>
    private IEnumerator InitRound(BattleStatus state)
    {
        state.Round++;
        state.BattleUIPanel.SetBoutCounterNumber(state.Round);
        yield return state.BattleUIPanel.AnimateInfoTextCallout($"Bout {state.Round}");
        Logger.Log(LogType.Combat, $"BOUT {state.Round} STARTED");

        // During the third bout and onward, army with low enough morale will run
        state.FleeingArmy = state.CheckForFleeingArmy(state.Round);
        if (state.FleeingArmy != null)
        {
            Logger.Log(LogType.Combat, $"Army {state.FleeingArmy.Faction.Name} is fleeing");
            yield break;
        }

        foreach (var combatant in state.Combatants)
        {
            // Process all start-turn effects (such as buffs) for durations.
            combatant.ProcessTurnStart();
        }

        state.Stage = BattleStatus.BattleStage.CombatConditionalEvents;
    }
}
