using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    /// <summary>
    /// Abstract base class for Animation Node with a single Output
    /// to another AnimationNodeBase.
    /// </summary>
    public abstract class SingleAnimationNode : AnimationNodeBase
    {
        [Output]
        public AnimationNodeBase Next;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Next)) return GetOutputPort(nameof(Next));
            else return base.GetValue(port);
        }

        public sealed override ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            var currentStep = GetAnimationStep(options);
            var nextNode = this.GetOutputPortValue<AnimationNodeBase>(nameof(Next));
            ICombatAnimationStep nextStep = nextNode?.GetStep(options);
            return new Runner(options, currentStep, nextStep);
        }

        /// <summary>
        /// Abstract impl of the animation step for the specific node, not accounting
        /// for next nodes.
        /// </summary>
        protected abstract ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options);

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
                    yield return _currentStep.PerformAction();
                }

                if (_nextStep != null)
                {
                    yield return _nextStep.PerformAction();
                }
            }
        }
    }

    /// <summary>
    /// Abstract base class for Animation Node with a single Output
    /// to another AnimationNodeBase, and a data object.
    /// </summary>
    public abstract class SingleAnimationNode<TData> : SingleAnimationNode
    {
        public TData Data;
    }

    /// <summary>
    /// Abstract base class for Animation Node with options, with a single Output
    /// to another AnimationNodeBase, and a data object of type CombatAnimationData.
    /// </summary>
    public abstract class SingleCombatAnimationDataNode<TData> : SingleAnimationNode<TData> where TData : CombatAnimationData
    {
        public CombatAnimationOptions Options;

        [ShowAssetPreview(32, 32)]
        [SerializeField]
        private Sprite _icon;

        private void OnValidate()
        {
            _icon = Data?.Icon;
        }

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            if (Data == null)
            {
                Debug.LogError($"No Data configured for animation node '{NodeName}'");
                return new AnimationStepEmptyRunner();
            }

            var opts = Options.Clone(options.FullTurn, options.Turn);
            return Data.Create(opts);
        }
    }
}
