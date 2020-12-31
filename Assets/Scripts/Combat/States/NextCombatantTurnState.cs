using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NextCombatantTurnState : BattleStateBase
{
    /// <summary>
    /// How long to stagger between parallel animations from
    /// same unit types.
    /// </summary>
    private float _parallelStaggerTime = 0.2f;

    public NextCombatantTurnState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are combatants in turn queue
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.CombatPhase && (state.TurnQueue.Count > 0 || state.AbilityQueue.Count > 0);
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        if (state.AbilityQueue.Count > 0)
        {
            // Queued officer ability
            var officer = state.GetPlayerOfficer();
            var ability = state.AbilityQueue.Dequeue();
            yield return state.BattleUIPanel.AnimateAbilityNameCallout(ability);
            var action = (new PreCombatCalculationEvent(state, officer, ability.Action)).Process();
            yield return DoTurn(state, new List<CombatAttackInfo>() { action });
        }
        else
        {
            // Queued combatant actions
            var actions = GetSimilarCombatActions(state);
            if (actions.Count > 0)
            {
                yield return DoTurn(state, actions);
            }
        }
    }

    /// <summary>
    /// Gets next action and any units in queue from same class
    /// expected to use the same attack.
    /// </summary>
    private List<CombatAttackInfo> GetSimilarCombatActions(BattleStatus state)
    {
        var current = state.GetNextCombatant();
        var original = current;
        if (current == null)
        {
            // nobody left over
            return new List<CombatAttackInfo>();
        }

        var actions = new List<CombatAttackInfo>();
        var action = (new PreCombatCalculationEvent(state, current)).Process();

        actions.Add(action);
        while (current != null)
        {
            current = state.PeekNextLiveCombatant();
            if (current == null)
            {
                break;
            }

            var newAction = new PreCombatCalculationEvent(state, current).Process();
            if (original.Allies == current.Allies &&
                newAction.Ability.MatchesOther(action.Ability))
            {
                // batch with others in same team that have same attack,
                // and dequeue the combatant
                actions.Add(newAction);
                state.GetNextCombatant();
            }
            else
            {
                // if the next doesn't match, just break out of the loop (done batching)
                break;
            }
        }

        return actions;
    }

    private IEnumerator DoTurn(BattleStatus state, List<CombatAttackInfo> actions)
    {
        var attackEvents = new CombatProcessEventSequence<CombatAttackEventResult>();
        var preAttackAnimations = new CombatAnimationEventSequence(_owner);
        var attackAnimations = new CombatAnimationEventSequence(_owner);
        var postAnimations = new CombatAnimationEventSequence(_owner);

        attackAnimations.StaggerTime = _parallelStaggerTime;

        // Update panel as needed
        var nonAllyActions = actions.Where(a => !a.IsAllyAbility).ToList();
        if (nonAllyActions.Count > 0)
        {
            state.BattleUIPanel.AttackStats.SetStats(nonAllyActions);
        }
        else
        {
            state.BattleUIPanel.AttackStats.Clear();
        }

        foreach (var action in actions)
        {
            preAttackAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, action, CombatAbilityAnimationEvent.AnimationType.PreAttack));
            attackAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, action, CombatAbilityAnimationEvent.AnimationType.Ability));
            attackEvents.AddEvent(new CombatAttackEvent(_owner, action));
            postAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, action, CombatAbilityAnimationEvent.AnimationType.PostAttack));
        }

        // Step 2: pre-ability animations
        yield return preAttackAnimations.Animate();

        // Step 3: ability animation
        yield return attackAnimations.Animate();

        // Step 4: process ability and get results (includes rolls etc)
        var attackEventResults = attackEvents.Process();
        foreach (var attackEventResult in attackEventResults)
        {
            foreach (var info in attackEventResult.Results)
            {
                yield return AnimateAttackResults(state, info);
            }
        }

        // Step 5: post ability animation
        yield return postAnimations.Animate();

        // Step 6: check for newly dead units
        yield return CheckForDeadUnits(state);

        state.BattleUIPanel.UpdateMorale(state.PlayerArmy, state.OtherArmy);

        // Brief pause
        yield return new WaitForSecondsAccelerated(1.0f);
    }

    private IEnumerator CheckForDeadUnits(BattleStatus state)
    {
        foreach (var combatant in state.Combatants.ToList())
        {
            
            if (combatant.Unit.IsDead())
            {
                yield return combatant.CombatUnit.AnimateDeath();
                state.ClearCombatant(combatant);

                // get morale bonus for killing unit
                var roll = UnityEngine.Random.Range(5, 10);

                // Positive morale change for attacking army
                combatant.Allies.Morale.ChangeMorale(roll);
                state.BattleUIPanel.AnimateMoraleBar(combatant.IsInPlayerArmy, true);
            }
        }
    }

    private IEnumerator AnimateAttackResults(BattleStatus state, ComputedAttackResultInfo info)
    {
        if (info.Target == null)
        {
            // Currently results with no target should probably just not animate
            yield break;
        }

        if (info.ArmyMoraleDamage.HasValue)
        {
            state.BattleUIPanel.AnimateMoraleBar(info.Target.IsInPlayerArmy, info.ArmyMoraleDamage.Value > 0);
        }

        // Prefer physical damage
        if (info.AttackDamage.HasValue)
        {
            var anim = new CombatUnitAnimationEvent(_owner, info);
            yield return anim.Animate();
        }

        if (info.DirectMoraleDamage.HasValue)
        {
            var anim = new CombatUnitAnimationEvent(_owner, info);
            yield return anim.Animate();
        }
    }
}

