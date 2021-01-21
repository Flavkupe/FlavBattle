using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FlavBattle.Combat.Events;

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
            var pickedAction = action ?? PickAction(state, combatant);
            var targets = PickTargets(state, combatant, pickedAction.Target);
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
                    target.ApplyStatChanges(effect);
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


            var action = PickAction(state, next);
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
        /// Picks an attack by priority, if one exists. If not, then returns null
        /// (meaning there is no preference).
        /// </summary>
        private CombatAction PickAction(BattleStatus state, Combatant combatant)
        {
            var actions = combatant.Unit.Info.Actions;

            // First filter by possible attacks, and return default if none are possible.
            var possible = FilterPossibleActions(state, combatant, actions);

            if (possible.Count == 0)
            {
                Debug.Log("No valid actions! Returning global default");
                return GameResourceManager.Instance.GetDefaultCombatAction();
            }

            // Get random action from the top priority possible action
            var maxPriority = possible.Max(a => a.Priority);
            return possible.Where(a => a.Priority == maxPriority).ToList().GetRandom();
        }

        private List<CombatAbilityPriority> GetPriorityValuesReversed()
        {
            var list = new List<CombatAbilityPriority>();
            foreach (CombatAbilityPriority priority in Enum.GetValues(typeof(CombatAbilityPriority)))
            {
                list.Add(priority);
            }

            list.Reverse();
            return list;
        }

        /// <summary>
        /// Checks whether any units are affected by the ability. Checks both positional
        /// and unit requirements of ability.
        /// </summary>
        private bool CanHitUnits(BattleStatus state, Combatant combatant, CombatAction ability)
        {
            return GetValidAbilityTargets(state, combatant, ability.Target).Count > 0;
        }


        /// <summary>
        /// Filters abilities by those that can target enemy formation
        /// </summary>
        private List<CombatAction> FilterPossibleActions(BattleStatus state, Combatant combatant, List<CombatAction> actions)
        {
            var enemyPositions = combatant.Enemies.Formation.GetOccupiedPositions(true);
            List<CombatAction> possible = new List<CombatAction>();
            foreach (var action in actions)
            {
                // Check if ability can hit any units
                if (CanHitUnits(state, combatant, action))
                {
                    possible.Add(action);
                }
            }

            return possible;
        }

        /// <summary>
        /// Gets the units that are affected by the ability. Checks both positional
        /// and unit requirements of ability.
        /// </summary>
        private List<Combatant> GetValidAbilityTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
        {
            var targetArmy = target.AffectsAllies() ? combatant.Allies : combatant.Enemies;
            var validPositions = FormationUtils.GetFormationPairs(target.ValidTargets);
            var validCombatants = state.GetCombatants(targetArmy.Formation.GetUnits(validPositions, true));
            if (target.ValidOpponent == ValidOpponent.Any)
            {
                return validCombatants;
            }

            if (target.ValidOpponent == ValidOpponent.LowerLevel)
            {
                return validCombatants.Where(a => a.Unit.Info.CurrentStats.Level < combatant.Unit.Info.CurrentStats.Level).ToList();
            }

            if (target.ValidOpponent == ValidOpponent.HigherLevel)
            {
                return validCombatants.Where(a => a.Unit.Info.CurrentStats.Level > combatant.Unit.Info.CurrentStats.Level).ToList(); ;
            }

            Debug.LogWarning($"No check configured for ability validity type {target.ValidOpponent}; treating as 'Any'");
            return validCombatants;
        }

        private List<Combatant> PickTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
        {
            var empty = new List<Combatant>();
            if (target.TargetType == CombatAbilityTarget.Self)
            {
                return new List<Combatant>() { combatant };
            }

            // TODO: pick best targets based on other things
            var units = GetValidAbilityTargets(state, combatant, target);
            if (units.Count == 0)
            {
                return empty;
            }

            switch (target.TargetType)
            {
                case CombatAbilityTarget.Self:
                    return new List<Combatant> { combatant };
                case CombatAbilityTarget.RandomEnemy:
                case CombatAbilityTarget.RandomAlly:
                    return new List<Combatant> { units.GetRandom() };
                case CombatAbilityTarget.AllAllies:
                case CombatAbilityTarget.AllEnemies:
                default:
                    return units;
            }
        }

        private int GetTotalAttack(Combatant combatant, CombatAbilityData ability)
        {
            var attack = combatant.CombatCombinedStats.Power;
            attack += ability.Damage.RandomBetween();
            attack += combatant.UnitMoraleBonus;
            if (combatant.Allies.Stance == FightingStance.Offensive)
            {
                attack += 1;
            }
            else if (combatant.Allies.Stance == FightingStance.Defensive)
            {
                attack -= 1;
            }

            return attack;
        }

        private int GetTotalDefense(Combatant combatant)
        {
            var defense = combatant.CombatCombinedStats.Defense;
            defense += combatant.UnitMoraleBonus;
            if (combatant.Allies.Stance == FightingStance.Offensive)
            {
                defense -= 1;
            }
            else if (combatant.Allies.Stance == FightingStance.Defensive)
            {
                defense += 1;
            }

            return defense;
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
            var defense = GetTotalDefense(target);
            var attack = GetTotalAttack(target, ability);
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