using FlavBattle.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat
{
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
                Debug.Log("No valid actions! Returning global default");
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
    }
}
