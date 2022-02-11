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
        public enum ConditionType
        {
            Default,
            Blocked,
        }

        protected override string NodeName => "Conditional";

        [SerializeField]
        private ConditionOptions _conditionOptions;

        [Output(dynamicPortList = true)]
        public Condition[] Conditions;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Conditions)) return GetOutputPort(nameof(Conditions));
            else return base.GetValue(port);
        }

        public override ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            var conditions = new List<ConditionNode>();
            for (var i = 0; i < Conditions.Length; i++)
            {
                var action = Conditions[i];
                var node = this.GetOutputPortValue<AnimationNodeBase>($"{nameof(Conditions)} {i}");
                var actionNode = new ConditionNode()
                {
                    ConditionType = action.ConditionType,
                    Node = node,
                };
                conditions.Add(actionNode);
            }

            return new Runner(options, conditions, _conditionOptions);
        }

        [Serializable]
        public class ConditionOptions
        {
            [Tooltip("If true, will stop processing actions on first conditional statement. If false, will continue processing each action.")]
            public bool StopOnFirstConditional = true;
        }

        [Serializable]
        public class Condition
        {
            public ConditionType ConditionType;
        }

        public class ConditionNode : Condition
        {
            public AnimationNodeBase Node;
        }

        private class Runner : AnimationStepRunnerBase
        {
            private List<ConditionNode> _nodes;
            private ConditionOptions _conditionOptions;

            public Runner(CombatAnimationOptions options, List<ConditionNode> nodes, ConditionOptions conditionOptions) : base(options)
            {
                _nodes = nodes;
                _conditionOptions = conditionOptions;
            }

            public override IEnumerator Do()
            {
                if (Options.Turn == null)
                {
                    Debug.LogWarning("Condition node used without Turn data!");
                    yield break;
                }

                foreach (var node in _nodes)
                {
                    if (IsConditionTrue(node.ConditionType))
                    {
                        var step = node.Node.GetStep(Options);
                        yield return step.PerformAction();
                        if (_conditionOptions.StopOnFirstConditional)
                        {
                            break;
                        }
                    }
                }
            }

            private bool IsConditionTrue(ConditionType conditionType)
            {
                var turn = Options.Turn;
                switch (conditionType)
                {
                    case ConditionType.Blocked:
                        return turn.ResistedAttack;
                    default:
                    case ConditionType.Default:
                        return true;
                }
            }
        }
    }
}
