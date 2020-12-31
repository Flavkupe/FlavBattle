using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PreCombatCalculationEvent : ICombatProcessEvent<CombatAttackInfo>
{
    private BattleStatus _state;
    private Combatant _combatant;

    public PreCombatCalculationEvent(BattleStatus state, Combatant combatant)
    {
        _state = state;
        _combatant = combatant;
    }

    public CombatAttackInfo Process()
    {
        var action = PickAction(_state, _combatant, _combatant.Unit.Info.Actions);
        var targets = PickTargets(_state, _combatant, action.Target);
        var info = GetAttackInfo(_state, _combatant, action, targets);
        return info;
    }
    
    private CombatAttackInfo GetAttackInfo(BattleStatus state, Combatant combatant, CombatAction action, List<Combatant> targets)
    {
        var info = new CombatAttackInfo();

        var combatantInfo = new ComputedAttackInfo()
        {
            Attack = GetTotalAttack(combatant, action),
            Combatant = combatant,
        };

        var targetsInfo = new List<ComputedAttackInfo>();
        foreach (var target in targets)
        {
            var targetInfo = new ComputedAttackInfo()
            {
                Defense = GetTotalDefense(target, action),
                Combatant = target,
            };

            targetsInfo.Add(targetInfo);
        }

        info.Targets = targetsInfo;
        info.Source = combatantInfo;
        info.Ability = action.Ability;
        info.TargetInfo = action.Target;
        info.State = state;
        return info;
    }

    private int GetTotalAttack(Combatant combatant, CombatAction action)
    {
        var attack = combatant.Unit.Info.CurrentStats.Power;
        attack += action.Ability.Damage.RandomBetween();
        attack += combatant.UnitMoraleBonus;
        return attack;
    }

    private int GetTotalDefense(Combatant combatant, CombatAction action)
    {
        var defense = combatant.Unit.Info.CurrentStats.Defense;
        defense += combatant.UnitMoraleBonus;
        return defense;
    }

    private List<Combatant> PickTargets(BattleStatus state, Combatant combatant, CombatTargetInfo target)
    {
        var empty = new List<Combatant>();
        if (target.TargetType == CombatAbilityTarget.Self)
        {
            return new List<Combatant>() { combatant };
        }

        // TODO: pick best targets based on other things
        //var targetPositions = targetArmy.Formation.GetOccupiedPositions(true);
        //var valid = FormationUtils.GetFormationPairs(ability.ValidTargets);
        //var targets = FormationUtils.GetIntersection(valid, targetPositions);
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
    /// Checks whether any units are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    private bool CanHitUnits(BattleStatus state, Combatant combatant, CombatAction ability)
    {
        return GetValidAbilityTargets(state, combatant, ability.Target).Count > 0;
    }

    /// <summary>
    /// Picks an attack by priority, if one exists. If not, then returns null
    /// (meaning there is no preference).
    /// </summary>
    private CombatAction PickAction(BattleStatus state, Combatant combatant, List<CombatAction> actions)
    {
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
}

