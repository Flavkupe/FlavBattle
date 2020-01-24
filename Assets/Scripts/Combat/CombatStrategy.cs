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
        var validStrats = _data.Strategies.Where(a => CanUseStrategy(a)).ToList();

        if (validStrats.Count == 0)
        {
            // TODO
            return new CombatStrategyDecision
            {
                Ability = null,
                Source = _unit,
                Targets = new List<Unit>(),
            };
        }

        // TODO: pick the best strat
        var chosenStrat = validStrats[0];
        var ability = PickAbility(chosenStrat);
        var targets = PickTargets(ability);
        return new CombatStrategyDecision
        {
            Ability = ability,
            Source = _unit,
            Targets = targets,
        };
    }

    private List<Unit> PickTargets(CombatAbilityData ability)
    {
        if (ability == null || !ability.IsTargetedAbility())
        {
            return new List<Unit>();
        }

        // TODO: pick best targets based on other things
        var preferred = FormationUtils.GetFormationPairs(ability.PreferredTargets);
        var enemyPositions = _enemies.Formation.GetOccupiedPositions(true);
        var targets = FormationUtils.GetIntersection(preferred, enemyPositions);
        if (targets.Count == 0)
        {
            var valid = FormationUtils.GetFormationPairs(ability.ValidTargets);
            targets = FormationUtils.GetIntersection(valid, enemyPositions);
        }

        var units = _enemies.Formation.GetUnits(targets);

        if (units.Count == 0)
        {
            return units;
        }

        switch(ability.Target) {
            case CombatAbilityTarget.RandomEnemy:
            case CombatAbilityTarget.RandomAlly:
                return new List<Unit> { units.GetRandom() };
            default:
                return units;
        }
    }

    private bool CanUseStrategy(CombatActionStrategy strategy)
    {
        if (strategy == CombatActionStrategy.Idle)
        {
            return true;
        }

        if (strategy == CombatActionStrategy.Flee)
        {
            // TODO
            return false;
        }

        return _unit.Info.Abilities.Any(a => a.Type.ToString() == strategy.ToString());
    }

    private CombatAbilityData PickAbility(CombatActionStrategy strategy)
    {
        var abilities = _unit.Info.Abilities.Where(a => a.Type.ToString() == strategy.ToString()).ToList();

        if (abilities.Count == 0)
        {
            // No abilities, pick default
            // TODO: check if default can actually be used
            Debug.Log("No abilities to pick from! Picking default");
            return _data.DefaultAbility;
        }

        // Defense
        if (strategy == CombatActionStrategy.Defend)
        {
            // TODO
            return abilities.GetRandom();
        }

        // Attack
        var priorities = _data.TargetPriorities;
        abilities = FilterPossibleAbilities(abilities);

        if (abilities.Count == 0)
        {
            Debug.Log("No possible abilities! Picking default");
            return _data.DefaultAbility;
        }

        // Sort by priority and pick attacks
        abilities.OrderByDescending(a => (int)a.Priority);

        foreach (var priority in priorities)
        {
            var attack = PickPreferredAttack(abilities, priority);
            if (attack != null)
            {
                return attack;
            }
        }

        var maxPriority = abilities.Select(a => a.Priority).Max();

        // No preferred choice; choose randomly, of highest priority items
        return abilities.Where(a => a.Priority == maxPriority).ToList().GetRandom();
    }

    /// <summary>
    /// Filters abilities by those that can target enemy formation
    /// </summary>
    private List<CombatAbilityData> FilterPossibleAbilities(List<CombatAbilityData> abilities)
    {
        var enemyPositions = _enemies.Formation.GetOccupiedPositions();
        List<CombatAbilityData> possible = new List<CombatAbilityData>();
        foreach (var ability in abilities)
        {
            // Check if any enemy position matches a valid position
            var validPositions = FormationUtils.GetFormationPairs(ability.ValidTargets);
            if (enemyPositions.Any(a => validPositions.Any(b => a.Equals(b))))
            {
                possible.Add(ability);
            }
        }

        return possible;
    }

    /// <summary>
    /// Picks an attack by priority, if one exists. If not, then returns null
    /// (meaning there is no preference).
    /// </summary>
    private CombatAbilityData PickPreferredAttack(List<CombatAbilityData> abilities, CombatTargetPriority priority)
    {
        CombatAbilityData ability = null;
        if (priority == CombatTargetPriority.Random)
        {
            return abilities.GetRandom();
        }
        else if (priority == CombatTargetPriority.Closest)
        {
            // TODO
        }
        else if (priority == CombatTargetPriority.Strongest)
        {
            // TODO
        }
        else if (priority == CombatTargetPriority.Weakest)
        {
            // TODO
        }
        else if (priority == CombatTargetPriority.FrontFirst ||
            priority == CombatTargetPriority.BackFirst)
        {
            ability = abilities.Where(a => PreferredTargetMatchesPriority(a.PreferredTargets, priority)).ToList().GetRandom();
        }

        return ability;
    }

    private bool PreferredTargetMatchesPriority(FormationGroup preferredTarget, CombatTargetPriority priority)
    {
        switch (priority)
        {
            case CombatTargetPriority.BackFirst:
                return FormationUtils.GetFormationPairs(preferredTarget).Any(a => a.Row == FormationRow.Back || a.Row == FormationRow.Middle);
            case CombatTargetPriority.FrontFirst:
                return FormationUtils.GetFormationPairs(preferredTarget).Any(a => a.Row == FormationRow.Front);
            default:
                return false;
        }
    }
}
