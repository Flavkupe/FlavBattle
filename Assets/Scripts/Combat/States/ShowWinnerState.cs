using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShowWinnerState : BattleStateBase
{
    public ShowWinnerState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are combatants in turn queue
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.CheckWinner() != BattleStatus.Winner.None;
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        yield return DoShowWinner(state);
    }

    /// <summary>
    /// Shows the winner of combat and hides all combat state
    /// </summary>
    /// <param name="winner"></param>
    /// <returns></returns>
    private IEnumerator DoShowWinner(BattleStatus state)
    {
        var winner = state.CheckWinner();
        var victory = winner == BattleStatus.Winner.Left;
        yield return state.BattleDisplay.ShowCombatEndSign(victory);
        yield return HideCombatUI(state);
        var winningArmy = victory ? state.PlayerArmy : state.OtherArmy;
        var losingArmy = state.GetOpponent(winningArmy);

        state.GameEventManager.TriggerCombatEndedEvent(winningArmy, losingArmy);
        state.Stage = BattleStatus.BattleStage.NotInCombat;
    }
}

