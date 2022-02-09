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
        CombatAnimationOptions Options { get; }

        IEnumerator Do();
    }

    /// <summary>
    /// Abstract base class for a simple class whose entire purpose is to
    /// call Do() with some options.
    /// </summary>
    public abstract class AnimationStepRunnerBase : ICombatAnimationStep
    {
        private CombatAnimationOptions _options;
        public CombatAnimationOptions Options => _options;

        public AnimationStepRunnerBase(CombatAnimationOptions options)
        {
            _options = options;
        }

        public abstract IEnumerator Do();
    }

    public abstract class CombatAnimationStepBase<TData> : ICombatAnimationStep where TData : CombatAnimationData
    {
        private TData _data;
        protected TData Data => _data;

        private CombatTurnUnitSummary _fullTurnSummary;
        protected CombatTurnUnitSummary FullTurnSummary => _fullTurnSummary;

        private CombatAnimationOptions _options;
        public CombatAnimationOptions Options => _options;

        protected Combatant Source => FullTurnSummary.Source;

        public CombatAnimationStepBase(TData data, CombatAnimationOptions options)
        {
            _data = data;
            _options = options;
            _fullTurnSummary = options.FullTurn;
        }

        /// <summary>
        /// Plays a random clip from array, if any.
        /// </summary>
        protected void PlaySound(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            var sound = clips.GetRandom();
            PlaySound(sound);
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            Sounds.Play(clip);
        }

        /// <summary>
        /// Performs a specific action.
        /// </summary>
        public abstract IEnumerator Do();

        /// <summary>
        /// Runs the task to completion, or in parallel, depending on value
        /// of WaitForCompletion in options.
        /// </summary>
        protected IEnumerator PerformAction(IEnumerator action)
        {
            // just call the extension function directly
            yield return action.PerformAction(Options);
        }
    }

    public abstract class CombatAnimationActionStepBase<TData> : CombatAnimationStepBase<TData> where TData : CombatAnimationData
    {
        private CombatTurnActionSummary _actionSummary;
        protected CombatTurnActionSummary ActionSummary => _actionSummary;

        protected Combatant Target
        {
            get
            {
                if (ActionSummary == null || ActionSummary.Target == null)
                {
                    Debug.LogError("Target specified for ability step without target! Using Source.");
                    return FullTurnSummary.Source;
                }

                return ActionSummary.Target;
            }
        }

        public CombatAnimationActionStepBase(TData data, CombatAnimationOptions options) : base(data, options)
        {
            _actionSummary = options.Turn;
        }
    }

    public static class ICombatAnimationStepExtensions
    {
        public static IEnumerator PerformAction(this ICombatAnimationStep step)
        {
            if (step == null)
            {
                yield break;
            }

            yield return step.Do().PerformAction(step.Options);
        }
    }
}
