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
        yield return DoPrebattleOfficerActions(state);
    }

    private IEnumerator DoPrebattleOfficerActions(BattleStatus state)
    {
        // TODO: enemy army as well
        var officer = state.GetPlayerOfficer();
        yield return DoOfficerActions(state, officer);
        state.Stage = BattleStatus.BattleStage.CombatPhase;
    }
}

