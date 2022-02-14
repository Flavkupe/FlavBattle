using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Combat.Animation.Nodes
{
    [Serializable]
    public class AnimationEffectNodeData
    {
        public PlayableAnimation Animation;

        public CombatAnimationOptions Options;
    }

    [CreateNodeMenu("Animation/Single/Effect")]
    public class AnimationEffectNode : SingleAnimationNode<AnimationEffectNodeData>
    {
        protected override string NodeName => throw new NotImplementedException();

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            var opts = Data.Options.Clone(options.FullTurn, options.Turn);
            return new Runner(opts, Data);
        }

        private class Runner : AnimationStepRunnerBase
        {
            private AnimationEffectNodeData _data;
            public Runner(CombatAnimationOptions options, AnimationEffectNodeData data) : base(options)
            {
                _data = data;
            }

            public override IEnumerator Do()
            {
                var subject = Options.Getsubject();
                var character = subject.CombatUnit.Character;
                var animation = Instantiate(_data.Animation, character.transform, false);
                animation.transform.localPosition = Vector3.zero;

                if (Options.WaitForCompletion)
                {
                    yield return animation.PlayToCompletion();
                }
                else
                {
                    animation.PlayAnimation();
                }
            }
        }
    }
}
