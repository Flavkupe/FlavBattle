using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Animates stuff like the unit getting hit and showing attack numbers
/// or other notifications.
/// </summary>
public class CombatUnitAnimationEvent : ICombatAnimationEvent
{
    private MonoBehaviour _owner;

    private ComputedAttackResultInfo _attackResult;

    public CombatUnitAnimationEvent(MonoBehaviour owner, ComputedAttackResultInfo attackResult)
    {
        _owner = owner;
        _attackResult = attackResult;
    }

    public IEnumerator Animate()
    {
        var target = _attackResult.Target;
        var slot = target.CombatFormationSlot;

        if (_attackResult.ShieldBlockedAttack || _attackResult.ResistedAttack)
        {
            // no yield
            slot.CurrentUnit.AnimateBlockedDamageAsync();
            slot.CurrentUnit.AnimateFlash(Color.yellow);
        }
        else if (_attackResult.MoraleBlockedAttack)
        {
            // no yield
            slot.CurrentUnit.AnimateBlockedThroughMoraleAsync();
            slot.CurrentUnit.AnimateFlash(Color.yellow);
        }
        else
        {
            if (_attackResult.AttackDamage.HasValue)
            {
                var damage = _attackResult.AttackDamage.Value.ToString();
                yield return slot.CurrentUnit.AnimateDamageTaken(damage, Color.red, Color.red);
            }

            if (_attackResult.DirectMoraleDamage.HasValue)
            {
                var damage = _attackResult.DirectMoraleDamage.Value.ToString();
                yield return slot.CurrentUnit.AnimateDamageTaken(damage, Color.blue, Color.blue);
            }
        }
    }
}