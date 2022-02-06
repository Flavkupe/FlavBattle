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

        private CombatTurnUnitSummary _fullTurnSummary;
        protected CombatTurnUnitSummary FullTurnSummary => _fullTurnSummary;

        private CombatAnimationOptions _options;
        protected CombatAnimationOptions Options => _options;

        protected Combatant Source => FullTurnSummary.Source;

        public CombatAnimationStepBase(TData data, CombatAnimationOptions options)
        {
            _data = data;
            _options = options;
            _fullTurnSummary = options.FullTurn;
        }

        /// <summary>
        /// Plays sounds before actions.
        /// </summary>
        protected void PlayPreSounds()
        {
            PlaySound(Data.PreAnimationSounds);
        }

        /// <summary>
        /// Plays sounds after actions.
        /// </summary>
        protected void PlayPostSounds()
        {
            PlaySound(Data.PostAnimationSounds);
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
        protected abstract IEnumerator DoAction();

        /// <summary>
        /// Plays does configued action, while playing sounds if configured.
        /// </summary>
        public IEnumerator Do()
        {
            PlayPreSounds();

            yield return DoAction();

            PlayPostSounds();
        }

        /// <summary>
        /// Runs the task to completion, or in parallel, depending on value
        /// of WaitForCompletion in options.
        /// </summary>
        protected IEnumerator PerformAction(IEnumerator action)
        {
            if (action == null)
            {
                yield break;
            }

            if (Options.WaitForCompletion)
            {
                yield return action;
            }
            else
            {
                var source = FullTurnSummary.Source.CombatUnit;
                source.StartCoroutine(action);
            }
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
}
