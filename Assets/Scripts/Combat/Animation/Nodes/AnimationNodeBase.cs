using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    public abstract class AnimationNodeBase : Node, ICombatAnimationNode
    {
        [Input]
        public AnimationNodeBase Previous;

        public string Description;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Previous)) return GetInputValue(nameof(Previous), Previous);
            else return base.GetValue(port);
        }

        protected abstract string NodeName { get; }

        protected virtual void OnReset()
        {
        }

        private void Reset()
        {
            name = NodeName;
            OnReset();
        }

        public abstract ICombatAnimationStep GetStep(CombatAnimationOptions options);
    }
}
