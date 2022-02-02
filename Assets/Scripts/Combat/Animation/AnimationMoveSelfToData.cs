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
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Move Self To", order = 1)]
    public class AnimationMoveSelfToData : CombatAnimationData
    {
        public CombatAbilityCharacterMoveEffect CharacterMoveToEffect;

        [ShowIf("ShowCharacterMoveArcProps")]
        public float CharacterMoveArcHeight;

        public float CharacterMoveSpeed;

        [Tooltip("Where to move relative to target")]
        public CombatAbilityCharacterMoveTarget CharacterMoveTarget;

        private bool ShowCharacterMoveArcProps()
        {
            return CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Arc;
        }

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationMoveSelfTo(this, options);
        }
    }

    public class AnimationMoveSelfTo : CombatAnimationActionStepBase<AnimationMoveSelfToData>
    {
        public AnimationMoveSelfTo(AnimationMoveSelfToData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        public override IEnumerator Do()
        {
            // find source position
            var sourceUnit = FullTurnSummary.Source.CombatUnit;
            var source = sourceUnit.Character;
            var sourcePos = source.transform.position;
            var originalPos = sourceUnit.OriginalPos.position;

            // find target position
            Vector3 targetPos;
            if (Data.CharacterMoveTarget == CombatAbilityCharacterMoveTarget.BackToSource)
            {
                targetPos = originalPos;
            }
            else
            {
                if (ActionSummary == null || ActionSummary.Target == null)
                {
                    Debug.LogWarning("Attempting target animation without a target! Moveing back to source.");
                    targetPos = originalPos;
                }
                else
                {
                    var target = ActionSummary.Target.CombatUnit;
                    targetPos = AnimationUtils.GetTargetPos(target, Data.CharacterMoveTarget, 0.5f);
                }
            }

            var speed = Data.CharacterMoveSpeed * Options.SpeedMultiplier;

            // do the move
            if (Data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Arc)
            {
                yield return AnimationUtils.MoveInArc(sourcePos, targetPos, source.gameObject, speed, Data.CharacterMoveArcHeight);
            }
            else if (Data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Straight)
            {
                // TODO
            }
            else
            {
                // Teleport
                source.transform.position = targetPos;
            }
        }
    }
}
