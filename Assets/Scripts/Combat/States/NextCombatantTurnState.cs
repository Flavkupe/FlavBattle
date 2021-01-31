using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FlavBattle.Combat.Events;
using FlavBattle.Entities;

namespace FlavBattle.Combat.States
{
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
                var summary = GenerateSpecificTurnSummary(state, officer, ability.Action);
                yield return DoTurn(state, summary);
            }
            else if (state.TurnQueue.Count > 0)
            {
                // Queued combatant actions
                var summary = GenerateNormalTurnSummary(state);
                yield return DoTurn(state, summary);
            }
        }

        private IEnumerator DoTurn(BattleStatus state, CombatTurnSummary summary)
        {
            var preAttackAnimations = new CombatAnimationEventSequence(_owner);
            var attackAnimations = new CombatAnimationEventSequence(_owner);
            var postAnimations = new CombatAnimationEventSequence(_owner);

            attackAnimations.StaggerTime = _parallelStaggerTime;

            // Update panel as needed
            state.BattleUIPanel.AttackStats.SetStats(summary);

            foreach (var turn in summary.Turns)
            {
                preAttackAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, turn, CombatAbilityAnimationEvent.AnimationType.PreAttack));
                attackAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, turn, CombatAbilityAnimationEvent.AnimationType.Ability));
                postAnimations.AddEvent(new CombatAbilityAnimationEvent(_owner, turn, CombatAbilityAnimationEvent.AnimationType.PostAttack));
            }

            // Step 2: pre-ability animations
            yield return preAttackAnimations.Animate();

            // Step 3: ability animation
            yield return attackAnimations.Animate();

            // Step 4: post ability animation
            yield return postAnimations.Animate();

            // Step 5: check for newly dead units
            ProcessDeadUnits(state);

            AnimateUIForResults(state, summary);

            // Brief pause
            yield return new WaitForSecondsAccelerated(1.0f);
        }

        private CombatTurnSummary GenerateSpecificTurnSummary(BattleStatus state, Combatant combatant, CombatAction action)
        {
            var summary = new CombatTurnSummary();
            var unitSummary = GenerateTurnSummaryForUnit(state, combatant, action);
            summary.Turns.Enqueue(unitSummary);
            return summary;
        }

        /// <summary>
        /// Pre-computes, processes and generates a turn summary for a normal unit's actions
        /// (not for an officer action)
        /// </summary>
        /// <returns></returns>
        private CombatTurnSummary GenerateNormalTurnSummary(BattleStatus state)
        {
            Debug.Assert(state.TurnQueue.Count > 0, "TurnQueue should be non-empty to get here!");

            var current = state.GetNextCombatant();
            var summary = new CombatTurnSummary();

            var firstResult = GenerateTurnSummaryForUnit(state, current);
            summary.Turns.Enqueue(firstResult);

            // Get others with same attack to go at same time
            Combatant next = null;
            while ((next = GetMatchingCombatantForTurn(state, current, firstResult.Ability)) != null)
            {
                var result = GenerateTurnSummaryForUnit(state, next);
                summary.Turns.Enqueue(result);
            }

            return summary;
        }

        private CombatTurnUnitSummary GenerateTurnSummaryForUnit(BattleStatus state, Combatant combatant, CombatAction action = null)
        {
            var summary = new CombatTurnUnitSummary();
            var pickedAction = action ?? CombatUtils.PickAction(state, combatant);
            var targets = CombatUtils.PickTargets(state, combatant, pickedAction.Target);
            summary.Source = combatant;
            summary.TargetInfo = pickedAction.Target;
            summary.Ability = pickedAction.Ability;

            var results = PerformAbilityOnCombatants(state, combatant, targets, pickedAction.Ability, pickedAction.Target);
            summary.Results.AddRange(results);
            return summary;
        }

        private List<CombatTurnActionSummary> PerformAbilityOnCombatants(BattleStatus state, Combatant combatant, List<Combatant> targets, CombatAbilityData ability, CombatTargetInfo targetInfo)
        {
            var actions = new List<CombatTurnActionSummary>();
            foreach (var target in targets)
            {
                var action = new CombatTurnActionSummary();
                action.Source = combatant;
                action.Ability = ability;
                action.Target = target;
                action.TileHighlight = targetInfo.AffectsAllies() ? Color.blue : Color.red;

                // TODO?
                var multiplier = 1.0f;

                // TODO: other effects
                if (ability.Effect.HasFlag(CombatAbilityEffect.StatusChange))
                {
                    var effect = ability.StatusEffect.Multiply(multiplier);
                    target.AddStatBuff(effect, ability.StatusEffectDuration);
                }

                if (ability.Effect.HasFlag(CombatAbilityEffect.Withdraw))
                {
                    state.FleeingArmy = target.Allies;
                }

                if (ability.Effect.HasFlag(CombatAbilityEffect.Damage))
                {
                    DealDirectDamageToTarget(action, combatant, target, ability);
                }

                if (ability.Effect.HasFlag(CombatAbilityEffect.MoraleDown))
                {
                    DealMoraleDamageToTarget(action, combatant, target, ability);
                }

                var moraleDamage = action.TotalMoraleDamage;
                if (moraleDamage > 0)
                {
                    DealMoraleDamageToArmy(action, combatant.Allies, target.Allies, moraleDamage);
                }

                actions.Add(action);
            }

            return actions;
        }

        private void ProcessDeadUnits(BattleStatus state)
        {
            foreach (var combatant in state.Combatants.ToList())
            {

                if (combatant.Unit.IsDead())
                {
                    combatant.CombatUnit.AnimateDeath();
                    state.ClearCombatant(combatant);

                    // get morale bonus for killing unit
                    var roll = UnityEngine.Random.Range(5, 10);

                    // Positive morale change for attacking army
                    combatant.Allies.Morale.ChangeMorale(roll);
                    state.BattleUIPanel.AnimateMoraleBar(combatant.IsInPlayerArmy, true);
                }
            }
        }

        /// <summary>
        /// Gets next combatant from the same team that is expected to do the same
        /// ability, in order to run its turn simultaneously.
        /// </summary>
        private Combatant GetMatchingCombatantForTurn(BattleStatus state, Combatant first, CombatAbilityData usedAbility)
        {
            Combatant next = state.PeekNextLiveCombatant();
            if (next == null)
            {
                return null;
            }

            if (first.Allies != next.Allies)
            {
                // not same team, so break now
                return null;
            }


            var action = CombatUtils.PickAction(state, next);
            if (action.Ability.MatchesOther(usedAbility))
            {
                // batch with others in same team that have same attack,
                // and dequeue the combatant
                state.GetNextCombatant();
                return next;
            }

            return null;
        }

        private void AnimateUIForResults(BattleStatus state, CombatTurnSummary summary)
        {
            if (summary.ArmyMoraleDamage > 0)
            {
                state.BattleUIPanel.AnimateMoraleBar(summary.FirstCombatant.IsInPlayerArmy, summary.ArmyMoraleDamage > 0);
            }

            state.BattleUIPanel.UpdateMorale(state.PlayerArmy, state.OtherArmy);
        }

        /// <summary>
        /// Deals morale damage to entire army (target) based on factors. source is
        /// opposing army (that is dealing morale damage). source and target can be null,
        /// depending on attack. Mutates summary with results.
        /// </summary>
        private void DealMoraleDamageToArmy(CombatTurnActionSummary summary, IArmy source, IArmy target, int unitMoraleDamage)
        {
            // TODO: affected by other stats
            // TODO: should mitigate under certain conditions
            var armyDamage = (int)Math.Max(1, (float)unitMoraleDamage / 5.0f);

            if (target != null)
            {
                // Negative morale change for attacked army
                target.Morale.ChangeMorale(-armyDamage);
            }

            summary.ArmyMoraleDamage = armyDamage;
        }

        /// <summary>
        /// Damages target with direct damage. Mutates summary with results.
        /// </summary>
        private void DealDirectDamageToTarget(CombatTurnActionSummary summary, Combatant attacker, Combatant target, CombatAbilityData ability)
        {
            // First update stat summaries
            attacker.UpdateStatSummaries();
            target.UpdateStatSummaries();

            var defense = target.Unit.StatSummary.GetTotal(UnitStatSummary.SummaryItemType.Defense);
            var attack = attacker.Unit.StatSummary.GetTotal(UnitStatSummary.SummaryItemType.Attack);
            attack += ability.Damage.RandomBetween();

            summary.Defense = defense;
            summary.Attack = attack;
            var targetMorale = target.UnitMorale;
            var moraleDamage = 10;
            var selfMoraleDamage = 0;

            var damage = ability.Damage.RandomBetween();
            if (attack > defense)
            {
                if (target.CombatCombinedStats.MoraleShields > 0)
                {
                    // tank the hit due to high morale (still take morale damage)
                    summary.MoraleBlockedAttack = true;
                    target.StatChanges.MoraleShields--;
                    damage = 0;
                }
            }
            else
            {
                summary.ResistedAttack = true;
                if (target.CombatCombinedStats.BlockShields > 0)
                {
                    // fully tank the hit
                    summary.ShieldBlockedAttack = true;
                    target.StatChanges.BlockShields--;
                    damage = 0;
                    moraleDamage = 0;
                    selfMoraleDamage = 5;
                }
                else
                {
                    // Halve damage and morale
                    damage = Math.Max(1, damage / 2);
                    moraleDamage = 5;
                }
            }

            summary.SelfMoraleDamage = selfMoraleDamage;
            summary.IndirectMoraleDamage = moraleDamage;
            summary.AttackDamage = damage;
            target.CombatUnit.TakeDamage(damage);
            target.CombatUnit.TakeMoraleDamage(moraleDamage);
            attacker.CombatUnit.TakeMoraleDamage(selfMoraleDamage);
        }

        /// <summary>
        /// Damages target with morale damage. Mutates summary with results.
        /// </summary>
        private void DealMoraleDamageToTarget(CombatTurnActionSummary summary, Combatant attacker, Combatant target, CombatAbilityData ability)
        {
            // TODO: morale damage mitigation based on bravery stats and other factors
            var moraleDamage = ability.MoraleDamage.RandomBetween();
            summary.DirectMoraleDamage = moraleDamage;
            target.CombatUnit.TakeMoraleDamage(moraleDamage);
        }
    }
}