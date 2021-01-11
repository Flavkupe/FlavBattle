using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.Events
{

    /// <summary>
    /// Animates stuff like the unit getting hit and showing attack numbers
    /// or other notifications.
    /// </summary>
    public class CombatUnitAnimationEvent : ICombatAnimationEvent
    {
        private MonoBehaviour _owner;

        private CombatTurnActionSummary _summary;

        public CombatUnitAnimationEvent(MonoBehaviour owner, CombatTurnActionSummary summary)
        {
            _owner = owner;
            _summary = summary;
        }

        public IEnumerator Animate()
        {
            var target = _summary.Target;
            var slot = target.CombatFormationSlot;
            var unit = slot.CurrentUnit;
            if (unit == null)
            {
                yield break;
            }

            if (_summary.ShieldBlockedAttack || _summary.ResistedAttack)
            {
                // no yield
                unit.AnimateBlockedDamageAsync();
                Sounds.Play(CombatSoundType.Block);
                // unit.AnimateFlash(Color.yellow);
            }
            else if (_summary.MoraleBlockedAttack)
            {
                // no yield
                unit.AnimateBlockedThroughMoraleAsync();
                unit.RemoveBuff(CombatBuffIcon.BuffType.MoraleShield);
                Sounds.Play(CombatSoundType.Block);
                // unit.AnimateFlash(Color.yellow);
            }
            else
            {
                if (_summary.AttackDamage > 0)
                {
                    var damage = _summary.AttackDamage.ToString();
                    yield return unit.AnimateDamageTaken(damage, Color.red, Color.red);

                }

                if (_summary.DirectMoraleDamage > 0)
                {
                    var damage = _summary.DirectMoraleDamage.ToString();
                    yield return unit.AnimateDamageTaken(damage, Color.blue, Color.blue);
                }
            }

            unit.UpdateUIComponents();
        }
    }
}