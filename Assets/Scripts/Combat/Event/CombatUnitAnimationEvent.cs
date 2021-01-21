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

            var damageToShow = _summary.AttackDamage;
            var moraleDamageToShow = _summary.DirectMoraleDamage;

            if (_summary.ResistedAttack)
            {
                if (_summary.ShieldBlockedAttack)
                {
                    damageToShow = 0;
                    unit.RemoveBuff(CombatBuffIcon.BuffType.BlockShield);
                    unit.AnimateFloatingIcon(CombatUnit.FloatingIconType.Shield);
                }

                // no yield
                unit.AnimateShieldBlock();
                Sounds.Play(CombatSoundType.Block);
            }
            else if (_summary.MoraleBlockedAttack)
            {
                damageToShow = 0;

                // no yield
                unit.AnimateFloatingIcon(CombatUnit.FloatingIconType.Morale);
                unit.RemoveBuff(CombatBuffIcon.BuffType.MoraleShield);
                Sounds.Play(CombatSoundType.Block);
            }

            if (damageToShow > 0)
            {
                yield return unit.AnimateDamageTaken(damageToShow.ToString(), Color.red, Color.red);
            }

            if (moraleDamageToShow > 0)
            {
                yield return unit.AnimateDamageTaken(moraleDamageToShow.ToString(), Color.red, Color.red);
            }

            unit.UpdateUIComponents();
        }
    }
}