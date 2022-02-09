using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Icon Float", order = 1)]
    public class AnimationIconFloatData : CombatAnimationData
    {
        public enum CharIconType
        {
            FromAbility,
            Specific,
        }

        [Tooltip("Template for the floaty. If Specific is the type, this will remain the floaty.")]
        [Required]
        public FloatingIcon FloatingIcon;

        [Tooltip("If Specific is used, will use the specific FloatingIcon value. Else will replace with ability.")]
        public CharIconType IconType = CharIconType.FromAbility;

        public Vector3 FloatingIconOffset = new Vector3(0.0f, 0.25f, 0.0f);

        public float FloatingIconScale = 0.25f;

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationIconFloat(this, options);
        }
    }

    public class AnimationIconFloat : CombatAnimationActionStepBase<AnimationIconFloatData>
    {
        public AnimationIconFloat(AnimationIconFloatData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        public override IEnumerator Do()
        {
            var subject = Options.Subject == CombatAnimationSubject.Source ? Source : Target;
            var character = subject.CombatUnit.Character;

            var floaty = GameObject.Instantiate(Data.FloatingIcon);

            if (Data.IconType == AnimationIconFloatData.CharIconType.FromAbility)
            {
                floaty.Sprite = Options.FullTurn.Ability.Icon;
            }

            floaty.transform.position = character.transform.position + Data.FloatingIconOffset;
            floaty.transform.localScale = floaty.transform.localScale * Data.FloatingIconScale;
            yield return PerformAction(floaty.PlayToCompletion());
        }
    }
}
