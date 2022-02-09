using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Conditional")]
    public class AnimationConditionalNode : AnimationNodeBase
    {
        protected override string NodeName => "Conditional";

        [Output(dynamicPortList = true)] public AnimationConditionalData.ConditionType[] Actions;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Actions)) return GetInputValue(nameof(Actions), Actions);
            else return base.GetValue(port);
        }

        public override ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            // TODO
            return null;
        }

        [Serializable]
        public class Condition
        {
            public AnimationConditionalData.ConditionType ConditionType;

            public AnimationNodeBase Node;
        }
    }
}
