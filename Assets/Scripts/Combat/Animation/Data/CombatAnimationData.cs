using FlavBattle.Components;
using System;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    public interface ICombatAnimationData
    {
        ICombatAnimationStep Create(CombatAnimationOptions options);
    }

    public abstract class CombatAnimationData : ScriptableObject
    {
        [AssetIcon]
        public Sprite Icon;

        [SerializeField]
        private string _description;

        public abstract ICombatAnimationStep Create(CombatAnimationOptions options);
    }

    public enum CombatAnimationSubject
    {
        Source,
        Target,
    }

    [Serializable]
    public class CombatAnimationDetails
    {
        public CombatAnimationData Data;
        public CombatAnimationOptions Options;

        /// <summary>
        /// Combines options with the ones specified by the 
        /// CombatAnimationDetails. FullTurn and Turn are inherited,
        /// but the rest is overridden.
        /// </summary>
        public ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            if (Data == null)
            {
                Debug.LogError($"No Data configured for animation");
                return new AnimationStepEmptyRunner();
            }

            var opts = Options.Clone();
            opts.FullTurn = options.FullTurn;
            opts.Turn = options.Turn;
            return Data.Create(opts);
        }
    }

    [Serializable]
    public class CombatAnimationOptions : ActionNodeGraphOptions
    {
        [Tooltip("Who is the subject of the animation.")]
        public CombatAnimationSubject Subject = CombatAnimationSubject.Source;

        public CombatTurnActionSummary Turn { get; set; }
        public CombatTurnUnitSummary FullTurn { get; set; }

        /// <summary>
        /// Clones this, optionally setting the fullTurn or turn, if provided.
        /// </summary>
        public CombatAnimationOptions Clone(CombatTurnUnitSummary fullTurn = null, CombatTurnActionSummary turn = null)
        {
            return new CombatAnimationOptions()
            {
                SpeedMultiplier = this.SpeedMultiplier,
                WaitForCompletion = this.WaitForCompletion,
                Subject = this.Subject,
                Turn = turn ?? this.Turn,
                FullTurn = fullTurn ?? this.FullTurn,
            };
        }

        public Combatant Getsubject()
        {
            if (Subject == CombatAnimationSubject.Target)
            {
                if (Turn?.Target != null)
                {
                    return Turn.Target;
                }

                Debug.LogError("Target selected as subject for Untargetted Animation tree! Using Source.");
            }

            return FullTurn.Source;
        }
    }
}
