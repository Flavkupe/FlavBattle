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
}

