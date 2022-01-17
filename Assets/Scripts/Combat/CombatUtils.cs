using FlavBattle.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat
{
    /// <summary>
    /// Large number of methods for processing various parts of combat.
    /// Methods here should not perform UI operations or animations, and
    /// should not contain IEnumerators or other awaitable functions.
    /// </summary>
    public static class CombatUtils
    {
        /// <summary>
        /// Checks whether any units are affected by the action. Checks both positional
        /// and unit requirements of ability.
        /// </summary>
        public static bool CanHitUnits(BattleStatus state, Combatant combatant, CombatAction action)
        {
            return GetValidAbilityTargets(state, combatant, action.Target).Count > 0;
        }

        /// <summary>
        /// Returns true if the action's stance requirements match the current stance.
        /// </summary>
        /// <returns></returns>
        public static bool HasProperStance(BattleStatus state, Combatant combatant, CombatAction action)
        {
            switch (combatant.Allies.Stance)
            {
                case FightingStance.Offensive:
                    return action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Offensive);
                case FightingStance.Defensive:
                    return action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Defensive);
                case FightingStance.Neutral:
                default:
                    return action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Neutral);
            }
        }

        public static List<Combatant> PickTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
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
                    return new List<Combatant> { PickBestFromList(state, combatant, units) };
                case CombatAbilityTarget.RandomAlly:
                    return new List<Combatant> { units.GetRandom() };
                case CombatAbilityTarget.AllAllies:
                case CombatAbilityTarget.AllEnemies:
                default:
                    return units;
            }
        }

        /// <summary>
        /// From the list of possible targets, gets the best target based on factors like
        /// column (prefers same column).
        /// </summary>
        private static Combatant PickBestFromList(BattleStatus state, Combatant combatant, List<Combatant> possibleTargets)
        {
            Debug.Assert(possibleTargets.Count > 0, "Calling PickBestFromList with no targets!");
            if (possibleTargets.Count == 1)
            {
                return possibleTargets[0];
            }

            // Prefer combatants from same or closest column
            var col = combatant.Col;
            var closest = possibleTargets.FirstOrDefault(a => a.Col == col);
            if (closest != null)
            {
                return closest;
            }

            // No enemy on same column, so go for next closest
            if (col != FormationColumn.Middle)
            {
                // can be either
                return possibleTargets.GetRandom();
            }
            else
            {
                // Is either middle or get a random target
                return possibleTargets.FirstOrDefault(a => a.Col == FormationColumn.Middle) ?? possibleTargets.GetRandom();
            }
        }

        /// <summary>
        /// Filters abilities by those that can target enemy formation
        /// </summary>
        public static List<CombatAction> FilterPossibleActions(BattleStatus state, Combatant combatant, List<CombatAction> actions)
        {
            var enemyPositions = combatant.Enemies.Formation.GetOccupiedPositions(true);
            List<CombatAction> possible = new List<CombatAction>();
            foreach (var action in actions)
            {
                // Filter out skills that require stance other than the current stance
                if (!HasProperStance(state, combatant, action))
                {
                    continue;
                }

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
        public static List<Combatant> GetValidAbilityTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
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

        /// <summary>
        /// Picks an attack by priority, if one exists. If not, then returns null
        /// (meaning there is no preference).
        /// </summary>
        public static CombatAction PickAction(BattleStatus state, Combatant combatant)
        {
            var actions = combatant.Unit.Info.Actions;

            // First filter by possible attacks, and return default if none are possible.
            var possible = FilterPossibleActions(state, combatant, actions);

            if (possible.Count == 0)
            {
                Logger.Log(LogType.Combat, "No valid actions! Returning global default");
                return GameResourceManager.Instance.GetDefaultCombatAction();
            }

            var maxPriority = possible.Max(a => a.Priority);
            var instant = possible.FirstOrDefault(a => a.InstantAbility && a.Priority == maxPriority);
            if (instant != null)
            {
                // If there is an instant ability with high priority possible, always use that...
                return instant;
            }

            // ... otherwise, get random action from the top priority possible action
            return possible.Where(a => a.Priority == maxPriority).ToList().GetRandom();
        }

        /// <summary>
        /// Generates a summary of activity for a turn for a unit (and possibly other units like it).
        /// </summary>
        /// <param name="state">Battle context.</param>
        /// <param name="combatant">The unit whose turn it is.</param>
        /// <param name="action">If non-null, a specific action to perform. If null, unit will pick an action from its available ones.</param>
        public static CombatTurnUnitSummary ProcessTurnForUnit(BattleStatus state, Combatant combatant, CombatAction action = null)
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

        /// <summary>
        /// Processes a combatant's turn and generates a summary of activity for the turn, using a specific pre-defined
        /// combatant and action.
        /// 
        /// This overload will most likely be used to invoke a specific enqueued action such as an officer action.
        /// </summary>
        /// <param name="state">Battle context.</param>
        /// <param name="combatant">The combatant that will generate the turn summary.</param>
        /// <param name="action">The action to be performed.</param>
        /// <returns></returns>
        public static CombatTurnSummary ProcessTurn(BattleStatus state, Combatant combatant, CombatAction action)
        {
            var summary = new CombatTurnSummary();
            var unitSummary = ProcessTurnForUnit(state, combatant, action);
            summary.Turns.Enqueue(unitSummary);
            return summary;
        }


        /// <summary>
        /// Pre-computes, processes and generates a turn and returns a summary for a normal unit's
        /// actions, based on the next unit in the TurnQueue.
        /// 
        /// For officer actions, should use the other overload.
        /// </summary>
        /// <returns></returns>
        public static CombatTurnSummary ProcessTurn(BattleStatus state)
        {
            if (state.TurnQueue.Count == 0)
            {
                Debug.LogWarning("Trying to Generate turn Summary with no TurnQueue items.");
                return null;
            }

            var current = state.GetNextCombatant();
            var summary = new CombatTurnSummary();

            var firstResult = ProcessTurnForUnit(state, current);
            summary.Turns.Enqueue(firstResult);

            // Get others with same attack to go at same time
            Combatant next;
            while ((next = GetMatchingCombatantForTurn(state, current, firstResult.Ability)) != null)
            {
                var result = ProcessTurnForUnit(state, next);
                summary.Turns.Enqueue(result);
            }

            return summary;
        }

        /// <summary>
        /// Gets next combatant from the same team that is expected to do the same
        /// ability, in order to run its turn simultaneously.
        /// </summary>
        public static Combatant GetMatchingCombatantForTurn(BattleStatus state, Combatant first, CombatAbilityData usedAbility)
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

        public static List<CombatTurnActionSummary> PerformAbilityOnCombatants(BattleStatus state, Combatant combatant, List<Combatant> targets, CombatAbilityData ability, CombatTargetInfo targetInfo)
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
                    target.AddStatBuff(ability.Name, effect, ability.StatusEffectDuration);
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

        /// <summary>
        /// Damages target with direct damage. Mutates summary with results.
        /// </summary>
        public static void DealDirectDamageToTarget(CombatTurnActionSummary summary, Combatant attacker, Combatant target, CombatAbilityData ability)
        {
            var targetStatSummary = target.GetUnitStatSummary();
            var attackerStatSummary = attacker.GetUnitStatSummary();
            var defense = targetStatSummary.GetTotal(UnitStatType.Defense);
            var attack = attackerStatSummary.GetTotal(UnitStatType.Power);
            attack += ability.Damage.RandomBetween();

            summary.Defense = defense;
            summary.Attack = attack;
            var moraleDamage = 10;
            var selfMoraleDamage = 0;

            var currentStats = target.Unit.Info.CurrentStats;

            var damage = ability.Damage.RandomBetween();
            if (attack > defense)
            {
                if (currentStats.ActiveMoraleShields > 0)
                {
                    // tank the hit due to high morale (still take morale damage)
                    summary.MoraleBlockedAttack = true;
                    currentStats.ActiveMoraleShields--;
                    damage = 0;
                }
            }
            else
            {
                summary.ResistedAttack = true;
                if (currentStats.ActiveBlockShields > 0)
                {
                    // fully tank the hit
                    summary.ShieldBlockedAttack = true;
                    currentStats.ActiveBlockShields--;
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
            target.TakeDamage(damage);
            target.TakeMoraleDamage(moraleDamage);
            attacker.TakeMoraleDamage(selfMoraleDamage);
        }

        /// <summary>
        /// Damages target with morale damage. Mutates summary with results.
        /// </summary>
        public static void DealMoraleDamageToTarget(CombatTurnActionSummary summary, Combatant attacker, Combatant target, CombatAbilityData ability)
        {
            // TODO: morale damage mitigation based on bravery stats and other factors
            var moraleDamage = ability.MoraleDamage.RandomBetween();
            summary.DirectMoraleDamage = moraleDamage;
            target.TakeMoraleDamage(moraleDamage);
        }

        /// <summary>
        /// Deals morale damage to entire army (target) based on factors. source is
        /// opposing army (that is dealing morale damage). source and target can be null,
        /// depending on attack. Mutates summary with results.
        /// </summary>
        public static void DealMoraleDamageToArmy(CombatTurnActionSummary summary, IArmy source, IArmy target, int unitMoraleDamage)
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
        /// Clears out any units that are dead and returns a list of any
        /// units who have died.
        /// </summary>
        public static List<Combatant> ProcessDeadUnits(BattleStatus state)
        {
            var deadUnits = new List<Combatant>();
            foreach (var combatant in state.Combatants.ToList())
            {
                if (combatant.Unit.IsDead())
                {
                    deadUnits.Add(combatant);
                    state.ClearCombatant(combatant);

                    // get morale bonus for killing unit
                    var roll = UnityEngine.Random.Range(5, 10);

                    // Positive morale change for dead unit's enemies
                    combatant.Enemies.Morale.ChangeMorale(roll);

                    // get morale bonus for losing unit
                    roll = UnityEngine.Random.Range(5, 10);

                    // Negative morale change for dead unit's allies
                    combatant.Allies.Morale.ChangeMorale(-roll);
                }
            }

            return deadUnits;
        }
    }
}
