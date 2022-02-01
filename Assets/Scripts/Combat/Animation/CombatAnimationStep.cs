using System;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    /// <summary>
    /// A full step of a combat animation, such as attacking with a sword.
    /// </summary>
    public interface ICombatAnimationStep
    {
        IEnumerator Do();
    }

    public abstract class CombatAnimationStepBase<TData> : ICombatAnimationStep where TData : CombatAnimationData
    {
        private TData _data;
        protected TData Data => _data;

        public CombatAnimationStepBase(TData data)
        {
            _data = data;
        }

        public abstract IEnumerator Do();
    }

    public abstract class CombatAnimationActionStepBase<TData> : CombatAnimationStepBase<TData> where TData : CombatAnimationData
    {
        private CombatTurnActionSummary _actionSummary;
        protected CombatTurnActionSummary ActionSummary => _actionSummary;

        private CombatAnimationOptions _options;
        protected CombatAnimationOptions Options => _options;

        public CombatAnimationActionStepBase(TData data, CombatAnimationOptions options) : base(data)
        {
            _options = options;
            _actionSummary = options.Turn;
        }
    }
}
