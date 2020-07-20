using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public struct CombatStrategyDecision
{
    public CombatAbilityData Ability;
    public Unit Source;
    public List<Unit> Targets;
}

public class CombatStrategy
{
    private ICombatStrategy _data;
    private Unit _unit;
    private IArmy _allies;
    private IArmy _enemies;

    public CombatStrategy(ICombatStrategy data, Unit unit, IArmy allies, IArmy enemies)
    {
        _data = data;
        _unit = unit;
        _allies = allies;
        _enemies = enemies;
    }

    public CombatStrategyDecision Decide()
    {
        // TODO: pick the strat based on commands
        var ability = PickAbility(_unit.Info.Abilities.ToList(), _data.Strategies, _data.TargetPriority);
        var targets = PickTargets(ability);
        return new CombatStrategyDecision
        {
            Ability = ability,
            Source = _unit,
            Targets = targets,
        };
    }

    public List<Unit> PickTargets(CombatAbilityData ability)
    {
        var empty = new List<Unit>();
        if (ability == null || !ability.IsTargetedAbility())
        {
            return empty;
        }

        var targetArmy = ability.AffectsAllies() ? _allies : _enemies;

        // TODO: pick best targets based on other things
        //var targetPositions = targetArmy.Formation.GetOccupiedPositions(true);
        //var valid = FormationUtils.GetFormationPairs(ability.ValidTargets);
        //var targets = FormationUtils.GetIntersection(valid, targetPositions);
        var units = GetValidAbilityTargets(ability, targetArmy);
        if (units.Count == 0)
        {
            return empty;
        }

        switch(ability.Target) {
            case CombatAbilityTarget.RandomEnemy:
            case CombatAbilityTarget.RandomAlly:
                return new List<Unit> { units.GetRandom() };
            case CombatAbilityTarget.AllAllies:
            case CombatAbilityTarget.AllEnemies:
            default:
                return units;
        }
    }

    /// <summary>
    /// Filters abilities by those that can target enemy formation
    /// </summary>
    private List<CombatAbilityData> FilterPossibleAbilities(List<CombatAbilityData> abilities)
    {
        
        var enemyPositions = _enemies.Formation.GetOccupiedPositions(true);
        List<CombatAbilityData> possible = new List<CombatAbilityData>();
        foreach (var ability in abilities)
        {
            // Check if ability can hit any units
            if (CanAbilityHitUnits(ability, _enemies))
            {
                possible.Add(ability);
            }
        }

        return possible;
    }

    /// <summary>
    /// Checks whether any units are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    private bool CanAbilityHitUnits(CombatAbilityData ability, IArmy army)
    {
        return GetValidAbilityTargets(ability, army).Count > 0;
    }

    /// <summary>
    /// Gets the units that are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    private List<Unit> GetValidAbilityTargets(CombatAbilityData ability, IArmy army)
    {
        var validPositions = FormationUtils.GetFormationPairs(ability.ValidTargets);
        var validUnits = army.Formation.GetUnits(validPositions, true);
        if (ability.ValidOpponent == ValidOpponent.Any)
        {
            return validUnits;
        }

        if (ability.ValidOpponent == ValidOpponent.LowerLevel)
        {
            return validUnits.Where(a => a.Info.CurrentStats.Level < _unit.Info.CurrentStats.Level).ToList();
        }
        else if (ability.ValidOpponent == ValidOpponent.HigherLevel)
        {
            return validUnits.Where(a => a.Info.CurrentStats.Level > _unit.Info.CurrentStats.Level).ToList(); ;
        }

        Debug.LogWarning($"No check configured for ability validity type {ability.ValidOpponent}; treating as 'Any'");
        return validUnits;
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
    /// Picks an attack by priority, if one exists. If not, then returns null
    /// (meaning there is no preference).
    /// </summary>
    private CombatAbilityData PickAbility(List<CombatAbilityData> abilities, CombatActionStrategy[] strats, CombatTargetPriority targetPriority)
    {
        /**
         * The rules for attack presedence:
         * 
         * 0) Group abilities by strategy priority first
         * 1) For those groups, further group them by priority, from highest priority to lowest
         * 2) For each priority, see if an attack matches target priorities first. Pick those.
         * 3) If nothing matches preferred priority, pick any random valid attack for that group.
         * 4) If no attacks are valid for that group, check the next group.
         * 5) If no attack is preferred, pick unit's default.
         */

        // First filter by possible attacks, and return default if none are possible.
        abilities = FilterPossibleAbilities(abilities);

        if (abilities.Count == 0)
        {
            Debug.Log("No valid abilities! Picking default");
            return _data.DefaultAbility;
        }

        foreach (var strat in strats)
        {
            var stratGroup = abilities.Where(a => a.MatchesStrat(strat)).ToList();
            foreach (CombatAbilityPriority priority in GetPriorityValuesReversed())
            {
                var abilityGroup = stratGroup.Where(a => a.Priority == priority).ToList();
                if (abilityGroup.Count == 0)
                {
                    continue;
                }

                // First check if an attack is preferred based on target priority
                var pickedAbility = PickPreferredAttackByTargetPriority(abilityGroup, targetPriority);
                if (pickedAbility != null)
                {
                    // If an attack is preferred based on priority, pick that one
                    return pickedAbility;
                }

                // If no preference due to priority, pick a random attack from this tier
                return abilityGroup.GetRandom();
            }
        }

        // nothing found, so use default. This shouldn't happen but is a fallback
        Debug.LogWarning("No preferred abilities! Picking default; note: this shouldn't happen");
        return _data.DefaultAbility;
    }

    /// <summary>
    /// Given a filtered set of attacks, picks one based only on targetPriority
    /// </summary>
    /// <returns></returns>
    private CombatAbilityData PickPreferredAttackByTargetPriority(List<CombatAbilityData> abilities, CombatTargetPriority priority)
    {
        if (priority == CombatTargetPriority.Random)
        {
            // Random priority just gets a random attack
            return abilities.GetRandom();
        }
        else if (priority == CombatTargetPriority.Closest)
        {
            // TODO
            return abilities.GetRandom();
        }
        else if (priority == CombatTargetPriority.Strongest)
        {
            // TODO
            return abilities.GetRandom();
        }
        else if (priority == CombatTargetPriority.Weakest)
        {
            // TODO
            return abilities.GetRandom();
        }
        else if (priority == CombatTargetPriority.FrontFirst ||
            priority == CombatTargetPriority.BackFirst)
        {
            var ability = abilities.Where(a => ValidTargetsMatchesPriority(a.ValidTargets, priority)).ToList().GetRandom();
            if (ability != null)
            {
                return ability;
            }
        }

        // No preference on current priority
        return null;
    }

    private bool ValidTargetsMatchesPriority(FormationGroup validTargets, CombatTargetPriority priority)
    {
        switch (priority)
        {
            case CombatTargetPriority.BackFirst:
                return FormationUtils.GetFormationPairs(validTargets).Any(a => a.Row == FormationRow.Back || a.Row == FormationRow.Middle);
            case CombatTargetPriority.FrontFirst:
                return FormationUtils.GetFormationPairs(validTargets).Any(a => a.Row == FormationRow.Front);
            default:
                return false;
        }
    }
}
