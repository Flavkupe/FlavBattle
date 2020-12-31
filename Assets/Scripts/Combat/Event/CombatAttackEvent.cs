using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CombatAttackEventResult
{
    public List<ComputedAttackResultInfo> Results { get; } = new List<ComputedAttackResultInfo>();
}

public class CombatAttackEvent : ICombatProcessEvent<CombatAttackEventResult>
{
    private CombatAttackInfo _info;

    private MonoBehaviour _owner;

    private CombatAttackEventResult _results = new CombatAttackEventResult();

    public CombatAttackEvent(MonoBehaviour owner, CombatAttackInfo info)
    {
        _info = info;
        _owner = owner;
    }

    public CombatAttackEventResult Process()
    {
        var ability = _info.Ability;
        var targets = _info.Targets.Select(a => a.Combatant);
        var combatant = _info.Source.Combatant;

        foreach (var target in targets)
        {
            if (_info.TargetInfo.AffectsAllies())
            {
                PerformAbilityOnAllies(combatant, target, ability);
            }
            else
            {
                PerformAbilityOnEnemies(combatant, target, ability);
            }
        }

        return _results;
    }


    /// <summary>
    /// Gets a roll for morale, consisting of a random point between
    /// unit morale and his Allies' morale. It's a value from 0 to 100
    /// </summary>
    private int GetCombatantMoraleRoll(Combatant combatant)
    {
        var min = Math.Min(combatant.UnitMorale, combatant.ArmyMorale);
        var max = Math.Max(combatant.UnitMorale, combatant.ArmyMorale);
        return UnityEngine.Random.Range(min, max + 1); // +1 because Range top is exclusive
    }


    /// <summary>
    /// Gets unit stat bonuses associated with army.
    /// </summary>
    private UnitStats GetArmyBonusStats(IArmy army)
    {
        var stats = new UnitStats();
        if (army.Stance == FightingStance.Defensive)
        {
            // TEMP: simple stance bonuses for now
            stats.Defense += 1;
        }
        else if (army.Stance == FightingStance.Offensive)
        {
            // TEMP: simple stance bonuses for now
            stats.Power += 1;
        }

        return stats;
    }

    /// <summary>
    /// Given the 2 combatants and the damage dealt, calculates and returns Morale damage
    /// taken, accounting for all things.
    /// </summary>
    private int CalculateMoraleDamage(Combatant attacker, Combatant target, int damage)
    {
        // TODO: different morale damages
        // Currently taking half of damage as morale damage
        return damage / 2;
    }

    private void PerformAbilityOnAllies(Combatant combatant, Combatant target, CombatAbilityData ability)
    {
        // TODO?
        var multiplier = 1.0f;

        // TODO: other effects
        if (ability.Effect == CombatAbilityEffect.StatusChange)
        {
            var effect = ability.StatusEffect.Multiply(multiplier);
            target.ApplyStatChanges(effect);
        }
        else if (ability.Effect == CombatAbilityEffect.Withdraw)
        {
            _info.State.FleeingArmy = combatant.Allies;
        }
    }

    /// <summary>
    /// Deals morale damage to entire army (target) based on factors. source is
    /// opposing army (that is dealing morale damage). source and target can be null,
    /// depending on attack.
    /// </summary>
    private void DealMoraleDamageToArmy(IArmy source, IArmy target, int unitMoraleDamage)
    {
        // TODO: affected by other stats
        // TODO: should mitigate under certain conditions
        var armyDamage = (int)Math.Max(1, (float)unitMoraleDamage / 5.0f);

        if (target != null)
        {
            // Negative morale change for attacked army
            target.Morale.ChangeMorale(-armyDamage);
        }

        _results.Results.Add(new ComputedAttackResultInfo() { ArmyMoraleDamage = armyDamage });
    }

    /// <summary>
    /// Gets all combined target stats (with buffs etc) for combatant.
    /// Includes morale roll.
    /// </summary>
    private UnitStats GetCombinedCombatantStats(Combatant combatant)
    {
        var targetStats = combatant.Unit.Info.CurrentStats;
        var armyBonuses = GetArmyBonusStats(combatant.Allies);
        targetStats = targetStats.Combine(combatant.StatChanges, armyBonuses);

        // Calculate the morale effect on Combat stats.
        // Roll between -0.5 to 0.5, with 50 morale being 0. Multiply stats
        // by 1 + that number (range of 0.5 to 1.5, ie between 50% less to 50% more)
        var moraleRoll = GetCombatantMoraleRoll(combatant);
        moraleRoll -= 50;
        var moraleMultiplier = 1.0f + ((float)moraleRoll / 100.0f);
        targetStats.Multiply(moraleMultiplier);

        return targetStats;
    }

    private void PerformAbilityOnEnemies(Combatant attacker, Combatant target, CombatAbilityData ability)
    {
        var result = new ComputedAttackResultInfo(target);

        // TODO?
        var multiplier = 1.0f;

        var stats = GetCombinedCombatantStats(attacker);
        var slot = target.CombatFormationSlot;
        var targetStats = GetCombinedCombatantStats(target);

        var moraleDamage = 0;

        // TODO: other effects
        if (ability.Effect.HasFlag(CombatAbilityEffect.Damage))
        {
            var damage = 1;
            result.AttackDamage = damage;
            target.CombatUnit.TakeDamage(damage);

            // do some additional morale damage
            moraleDamage = CalculateMoraleDamage(attacker, target, damage);
        }

        if (ability.Effect.HasFlag(CombatAbilityEffect.MoraleDown))
        {
            // TODO: morale damage mitigation based on bravery stats and other factors
            moraleDamage += ability.MoraleDamage.RandomBetween();
            result.DirectMoraleDamage = moraleDamage;
            target.CombatUnit.TakeMoraleDamage(moraleDamage);
        }
        else if (moraleDamage > 0)
        {
            result.IndirectMoraleDamage = moraleDamage;
            target.CombatUnit.TakeMoraleDamage(moraleDamage);
        }

        _results.Results.Add(result);
        DealMoraleDamageToArmy(attacker?.Allies, target?.Allies, moraleDamage);
    }
}

