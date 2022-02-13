using FlavBattle.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Special/Start (Default)")]
    public class AnimationStartNode : Node, ICombatAnimationNode
    {
        [Output]
        public AnimationNodeBase Next;

        public ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            var nextNode = this.GetOutputPortValue<AnimationNodeBase>(nameof(Next));
            return nextNode?.GetStep(options);
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Next)) return GetOutputPort(nameof(Next));
            else return base.GetValue(port);
        }
    }
}
