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
        if (_info.TargetInfo.AffectsAllies())
        {
            PerformAbilityOnAllies(_info.Source, _info.Targets, _info.Ability);
        }
        else
        {
            PerformAbilityOnEnemies(_info.Source, _info.Targets, _info.Ability);
        }

        return _results;
    }

    private void PerformAbilityOnAllies(ComputedAttackInfo combatant, List<ComputedAttackInfo> targets, CombatAbilityData ability)
    {
        foreach (var target in targets)
        {
            // TODO?
            var multiplier = 1.0f;

            // TODO: other effects
            if (ability.Effect == CombatAbilityEffect.StatusChange)
            {
                var effect = ability.StatusEffect.Multiply(multiplier);
                target.Combatant.ApplyStatChanges(effect);
            }
            else if (ability.Effect == CombatAbilityEffect.Withdraw)
            {
                _info.State.FleeingArmy = combatant.Combatant.Allies;
            }
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

    private void PerformAbilityOnEnemies(ComputedAttackInfo attacker, List<ComputedAttackInfo> targets, CombatAbilityData ability)
    {
        foreach (var target in targets)
        {
            var result = new ComputedAttackResultInfo(target.Combatant);

            var slot = target.Combatant.CombatFormationSlot;

            var moraleDamage = 0;
            var targetMorale = target.Combatant.UnitMorale;
            if (ability.Effect.HasFlag(CombatAbilityEffect.Damage))
            {
                var damage = 0;
                if (attacker.Attack > target.Defense)
                {
                    if (targetMorale.GetTier() == Morale.Tier.High)
                    {
                        // tank the hit due to high morale
                        result.MoraleBlockedAttack = true;
                        moraleDamage += 10;
                    }
                    else
                    {
                        damage = 1;
                        moraleDamage += 5;
                    }
                }
                else
                {
                    result.ResistedAttack = true;
                }

                result.AttackDamage = damage;
                target.Combatant.CombatUnit.TakeDamage(damage);

                // do some additional morale damage
                moraleDamage += 5;
            }

            if (ability.Effect.HasFlag(CombatAbilityEffect.MoraleDown))
            {
                // TODO: morale damage mitigation based on bravery stats and other factors
                moraleDamage += ability.MoraleDamage.RandomBetween();
                result.DirectMoraleDamage = moraleDamage;
                target.Combatant.CombatUnit.TakeMoraleDamage(moraleDamage);
            }
            else if (moraleDamage > 0)
            {
                result.IndirectMoraleDamage = moraleDamage;
                target.Combatant.CombatUnit.TakeMoraleDamage(moraleDamage);
            }

            _results.Results.Add(result);
            DealMoraleDamageToArmy(attacker.Combatant.Allies, target.Combatant.Allies, moraleDamage);
        }
    }
}

