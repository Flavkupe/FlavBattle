using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ArmyFleeingState : BattleStateBase
{
    public ArmyFleeingState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are combatants in turn queue
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.FleeingArmy != null;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        yield return DoArmyFlee(state);
    }

    private IEnumerator DoArmyFlee(BattleStatus state)
    {
        var fleeing = state.FleeingArmy;
        state.BattleUIPanel.InfoTextCallout.SetText($"{fleeing.Faction.Name} is fleeing");
        yield return state.BattleUIPanel.InfoTextCallout.Animate();

        yield return AnimateArmyEscape(state, fleeing);
        var winner = state.GetOpponent(fleeing);

        state.GameEventManager.TriggerCombatEndedEvent(winner, fleeing, VictoryType.Fled);
        yield return HideCombatUI(state);
        state.Stage = BattleStatus.BattleStage.NotInCombat;
    }

    private IEnumerator AnimateArmyEscape(BattleStatus state, IArmy army)
    {
        var direction = army == state.PlayerArmy ? Vector3.left : Vector3.right;
        ParallelRoutineSet routines = new ParallelRoutineSet(_owner);
        foreach (var combatant in state.GetCombatants(army))
        {
            routines.AddRoutine(Routine.Create(combatant.CombatUnit.AnimateEscape(direction)));
        }

        yield return routines.ToRoutine();
    }
}

