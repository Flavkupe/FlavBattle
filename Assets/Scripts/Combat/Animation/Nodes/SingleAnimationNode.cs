using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    /// <summary>
    /// Abstract base class for Animation Node with a single Output
    /// to another AnimationNodeBase, and a data object.
    /// </summary>
    public abstract class SingleAnimationNode<TData> : AnimationNodeBase
    {
        [Output] public AnimationNodeBase Next;

        public TData Data;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Next)) return GetOutputPort(nameof(Next));
            else return base.GetValue(port);
        }

        private AnimationNodeBase GetAction(string fieldName)
        {
            return this.GetOutputPortValue<AnimationNodeBase>(fieldName);
        }

        /// <summary>
        /// Abstract impl of the animation step for the specific node, not accounting
        /// for next nodes.
        /// </summary>
        protected abstract ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options);

        public sealed override ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            var currentStep = GetAnimationStep(options);
            var nextNode = GetAction(nameof(Next));
            ICombatAnimationStep nextStep = nextNode?.GetStep(options);
            return new Runner(options, currentStep, nextStep);
        }

        private class Runner : AnimationStepRunnerBase
        {
            private ICombatAnimationStep _nextStep;
            private ICombatAnimationStep _currentStep;

            public Runner(CombatAnimationOptions options, ICombatAnimationStep currentStep, ICombatAnimationStep nextStep) : base(options)
            {
                _currentStep = currentStep;
                _nextStep = nextStep;
            }

            public override IEnumerator Do()
            {
                if (_currentStep != null)
                {
                    yield return _currentStep.Do().PerformAction(Options);
                }

                if (_nextStep != null)
                {
                    yield return _nextStep.Do().PerformAction(Options); ;
                }
            }

        }
    }

    /// <summary>
    /// Abstract base class for Animation Node with options, with a single Output
    /// to another AnimationNodeBase, and a data object of type CombatAnimationData.
    /// </summary>
    public abstract class SingleCombatAnimationDataNode<TData> : SingleAnimationNode<TData> where TData : CombatAnimationData
    {
        public CombatAnimationOptions Options;

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            var opts = Options.Clone();
            opts.FullTurn = options.FullTurn;
            opts.Turn = options.Turn;
            return Data.Create(opts);
        }
    }

    [CreateNodeMenu("Animation/Single/Projectile")]
    public class AnimationProjectileNode : SingleCombatAnimationDataNode<AnimationProjectileData>
    {
        protected override string NodeName => "Projectile";
    }

    [CreateNodeMenu("Animation/Single/CharDamage")]
    public class AnimationCharDamageNode : SingleCombatAnimationDataNode<AnimationCharDamageData>
    {
        protected override string NodeName => "Damage";
    }
}
