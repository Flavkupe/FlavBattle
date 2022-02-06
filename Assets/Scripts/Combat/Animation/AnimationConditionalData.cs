using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Conditional", order = 1)]
    public class AnimationConditionalData : CombatAnimationData
    {
        public enum ConditionType
        {
            Default,
            Blocked,
        }

        [Serializable]
        public class Condition
        {
            [Required]
            public CombatAnimationData Action;

            public ConditionType ConditionType;
        }

        [Tooltip("If true, will stop processing actions on first conditional statement. If false, will continue processing each action.")]
        public bool StopOnFirstConditional = true;

        public Condition[] ConditionalActions;

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationConditional(this, options);
        }
    }

    public class AnimationConditional : CombatAnimationActionStepBase<AnimationConditionalData>
    {
        public AnimationConditional(AnimationConditionalData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        protected override IEnumerator DoAction()
        {

            foreach (var condition in Data.ConditionalActions)
            {
                if (condition.Action != null && IsConditionTrue(condition.ConditionType))
                {
                    var action = condition.Action.Create(Options);
                    yield return PerformAction(action.Do());
                    if (Data.StopOnFirstConditional)
                    {
                        yield break;
                    }
                }
            }
        }

        private bool IsConditionTrue(AnimationConditionalData.ConditionType conditionType)
        {
            switch (conditionType)
            {
                case AnimationConditionalData.ConditionType.Blocked:
                    return ActionSummary.ResistedAttack;
                default:
                case AnimationConditionalData.ConditionType.Default:
                    return true;
            }
        }
    }
}
