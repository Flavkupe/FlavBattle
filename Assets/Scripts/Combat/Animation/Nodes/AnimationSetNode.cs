using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Set")]
    public class AnimationSetNode : AnimationNodeBase
    {
        [Serializable]
        public class Options
        {
            public bool Await = true;
        }

        public enum Sequence
        {
            Serial,
            Parallel,
        }

        public enum Subject
        {
            /// <summary>
            /// Events do not target results (eg, for Post/Pre actions)
            /// and are run sequentially regardless of Results.
            /// </summary>
            Untargeted,

            /// <summary>
            /// Runs all the actions repeated, for each Result
            /// </summary>
            ResultsRepeated,

            /// <summary>
            /// For each result, runs the next action, wrapping around the list.
            /// </summary>
            ResultsRoundRobin,
        }

        [Tooltip("How the result summary will be populated and targeted for the animation set.")]
        public Subject SubjectType;

        [Tooltip("How to process the parallel events.")]
        public Sequence SequenceType;

        [Tooltip("How long to wait between each event.")]
        public float Stagger = 0.0f;

        protected override string NodeName => "Set";

        [Output(dynamicPortList = true)] public Options[] Actions;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Actions)) return GetOutputPort(nameof(Actions));
            else return base.GetValue(port);
        }

        public override ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            return new Runner(options, this);
        }

        public class Runner : ICombatAnimationStep
        {
            private class NodeItem
            {
                public NodeItem(AnimationNodeBase node, bool await)
                {
                    Node = node;
                    Await = await;
                }

                public AnimationNodeBase Node;
                public bool Await;

                public ICombatAnimationStep GetStep(CombatAnimationOptions options)
                {
                    var clone = options.Clone();
                    clone.WaitForCompletion = Await;
                    return Node.GetStep(clone);
                }
            }

            private CombatAnimationOptions _options;
            private AnimationSetNode _data;
            public Runner(CombatAnimationOptions options, AnimationSetNode data)
            {
                _options = options;
                _data = data;
            }

            public CombatAnimationOptions Options => _options;

            public IEnumerator Do()
            {
                var fullTurn = _options.FullTurn;
                var subject = fullTurn.Source.CombatUnit;
                var actions = _data.Actions;
                var nodes = new List<NodeItem>();
                for (var i = 0; i < actions.Length; i++)
                {
                    var value = _data.GetOutputPortValue<AnimationNodeBase>($"Actions {i}");
                    if (value == null)
                    {
                        continue;
                    }

                    // value.GetStep(opts);

                    var node = new NodeItem(value, actions[i].Await);
                    nodes.Add(node);
                }

                if (_data.SubjectType == Subject.Untargeted)
                {
                    foreach (var node in nodes)
                    {
                        var step = node.GetStep(Options);
                        yield return step.PerformAction();
                        if (_data.Stagger > 0.0f)
                        {
                            yield return new WaitForSeconds(_data.Stagger);
                        }
                    }
                }
                else if (_data.SubjectType == Subject.ResultsRepeated)
                {
                    foreach (var result in fullTurn.Results)
                    {
                        foreach (var node in nodes)
                        {
                            Options.Turn = result;
                            var step = node.GetStep(Options);
                            yield return step.PerformAction();
                            if (_data.Stagger > 0.0f)
                            {
                                yield return new WaitForSeconds(_data.Stagger);
                            }
                        }
                    }
                }
                else if (_data.SubjectType == Subject.ResultsRoundRobin)
                {
                    for (var i = 0; i < fullTurn.Results.Count; i++)
                    {
                        var node = nodes[i % fullTurn.Results.Count];
                        Options.Turn = fullTurn.Results[i];
                        var step = node.GetStep(Options);
                        yield return step.PerformAction();
                        if (_data.Stagger > 0.0f)
                        {
                            yield return new WaitForSeconds(_data.Stagger);
                        }
                    }
                }
            }
        }
    }
}
