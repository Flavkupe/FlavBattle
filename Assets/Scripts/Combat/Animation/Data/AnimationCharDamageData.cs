using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Damage Flash", order = 1)]
    public class AnimationCharDamageData : CombatAnimationData
    {
        public enum CharDamageSoundType
        {
            None = 1,

            FromAbility = 2,

            // TODO
            FromWeapon = 3,

            Specific = 4,
        }

        public Color FlashColor;

        public FloatingText FloatingText;

        public Vector3 FloatingTextOffset = new Vector3(0.0f, 1.0f, 0.0f);

        public CharDamageSoundType SoundType = CharDamageSoundType.FromAbility;

        [Tooltip("Random sound clip to play, if Specific is selected.")]
        [ShowIf("ShowSpecificAudio")]
        public AudioClip[] Clips;

        private bool ShowSpecificAudio()
        {
            return SoundType == CharDamageSoundType.Specific;
        }

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationCharDamage(this, options);
        }
    }

    public class AnimationCharDamage : CombatAnimationActionStepBase<AnimationCharDamageData>
    {
        public AnimationCharDamage(AnimationCharDamageData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        public override IEnumerator Do()
        {
            var subject = Options.Subject == CombatAnimationSubject.Source ? ActionSummary.Source : ActionSummary.Target;
            var character = subject.CombatUnit.Character;

            if (Data.SoundType == AnimationCharDamageData.CharDamageSoundType.FromAbility)
            {
                var hitSounds = FullTurnSummary.Ability.HitSoundClips;
                PlaySound(hitSounds);
            }
            else if (Data.SoundType == AnimationCharDamageData.CharDamageSoundType.Specific)
            {
                PlaySound(Data.Clips);
            }
            else
            {
                // TODO
            }

            if (Data.FloatingText != null)
            {
                var text = GameObject.Instantiate(Data.FloatingText);
                text.transform.position = character.transform.position + Data.FloatingTextOffset;
                text.SetText(ActionSummary.AttackDamage.ToString());
            }

            subject.CombatUnit.UpdateUIComponents();

            if (subject.CombatUnit.Unit.IsDead())
            {
                subject.CombatUnit.AnimateDeath();
            }

            yield return character.FlashColor(Data.FlashColor, 8.0f * Options.SpeedMultiplier);
        }
    }
}
