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
        public Color FlashColor;

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
            yield return character.FlashColor(Data.FlashColor, 8.0f * Options.SpeedMultiplier);   
        }
    }
}
