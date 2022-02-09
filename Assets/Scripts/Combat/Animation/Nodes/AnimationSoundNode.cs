using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [Serializable]
    public class AnimationSoundNodeData
    {
        public AudioClip[] Clips;
    }

    [CreateNodeMenu("Animation/Single/Sound")]
    public class AnimationSoundNode : SingleAnimationNode<AnimationSoundNodeData>
    {
        protected override string NodeName => "Sound";

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            return new Runner(options, Data);
        }

        private class Runner : AnimationStepRunnerBase
        {
            private AnimationSoundNodeData _data;
            public Runner(CombatAnimationOptions options, AnimationSoundNodeData data) : base(options)
            {
                _data = data;
            }

            public override IEnumerator Do()
            {
                Sounds.PlayRandom(_data.Clips);
                yield return null;
            }
        }
    }
}
