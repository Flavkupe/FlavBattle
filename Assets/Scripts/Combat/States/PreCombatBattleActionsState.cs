using FlavBattle.Combat;
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
        PrepareStats(state);
        yield return null;
        state.Stage = BattleStatus.BattleStage.CombatPhase;
    }

    private void PrepareStats(BattleStatus state)
    {
        foreach (var combatant in state.Combatants)
        {
            PrepareUnitShields(combatant);
            combatant.ApplyPerkStatBonuses();
        }
    }

    private void PrepareUnitShields(Combatant combatant)
    {
        // High morale shield - uncomment later?
        //if (combatant.UnitMorale.GetTier() == Morale.Tier.High)
        //{
        //    combatant.ApplyStatChanges(new UnitStats()
        //    {
        //        MoraleShields = 1
        //    });

        //    combatant.CombatUnit.AddBuff(CombatBuffIcon.BuffType.MoraleShield);
        //}

        combatant.AddBlockShields(combatant.Unit.Info.CurrentStats.StartingBlockShields);
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
        }
        else
        {
            Debug.Log($"{combatant.Unit.Info.Faction}: officer {combatant.Unit.Info.Name} has no officer actions!");
        }
    }
}
