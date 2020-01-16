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
    private CombatStrategyData _data;
    private Unit _unit;
    private Army _allies;
    private Army _enemies;

    public CombatStrategy(CombatStrategyData data, Unit unit, Army allies, Army enemies)
    {
        _data = data;
        _unit = unit;
        _allies = allies;
        _enemies = enemies;
    }

    public CombatStrategyDecision Decide()
    {
        var validStrats = _data.DefaultStrategy.Where(a => CanUseStrategy(a)).ToList();

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

        var ability = PickAbility(validStrats[0]);

        var targets = PickTargets(ability);
        return new CombatStrategyDecision
        {
            Ability = null,
            Source = _unit,
            Targets = targets,
        };
    }

    private List<Unit> PickTargets(CombatAbilityData ability)
    {
        // TODO
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
        if (strategy != CombatActionStrategy.Attack && strategy != CombatActionStrategy.Defend)
        {
            return null;
        }

        var abilities = _unit.Info.Abilities.Where(a => a.Type.ToString() == strategy.ToString()).ToList();

        if (abilities.Count == 0)
        {
            // No abilities
            return null;
        }

        // Defense
        if (strategy == CombatActionStrategy.Defend)
        {
            // TODO
            return abilities.GetRandom();
        }

        // Attack
        var priorities = _data.DefaultTargetPriority;
        abilities = FilterPossibleAbilities(abilities);
        foreach (var priority in priorities)
        {
            var attack = PickPreferredAttack(abilities, priority);
            if (attack != null)
            {
                return attack;
            }
        }

        // No preferred choice; choose randomly
        return abilities.GetRandom();
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

        return abilities;
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
        else if (priority == CombatTargetPriority.FrontFirst)
        {
            ability = abilities.FirstOrDefault(a => FormationUtils.IncludesRow(a.PreferredTargets, FormationRow.Front));
        }
        else if (priority == CombatTargetPriority.MiddleFirst)
        {
            ability = abilities.FirstOrDefault(a => FormationUtils.IncludesRow(a.PreferredTargets, FormationRow.Middle));
        }
        else if (priority == CombatTargetPriority.BackFirst)
        {
            ability = abilities.FirstOrDefault(a => FormationUtils.IncludesRow(a.PreferredTargets, FormationRow.Back));
        }

        return ability;
    }
}
