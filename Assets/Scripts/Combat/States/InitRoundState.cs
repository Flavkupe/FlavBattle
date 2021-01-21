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
        Debug.Log($"BOUT {state.Round} STARTED");

        if (state.Round >= 3)
        {
            // During the third bout and onward, army with low enough morale will run
            state.FleeingArmy = state.CheckForFleeingArmy();
            if (state.FleeingArmy != null)
            {
                Debug.Log($"Army {state.FleeingArmy.Faction.Name} is fleeing");
                yield break;
            }
        }

        foreach (var item in state.Combatants.OrderBy(a => a.CombatCombinedStats.Speed).Reverse())
        {
            state.TurnQueue.Enqueue(item);
        }

        state.Stage = BattleStatus.BattleStage.SelectStance;
    }
}

