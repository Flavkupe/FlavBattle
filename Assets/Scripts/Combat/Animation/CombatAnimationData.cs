using System;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
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

        public ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            var opts = Options.Clone();
            opts.FullTurn = options.FullTurn;
            opts.Turn = options.Turn;
            return Data.Create(opts);
        }
    }

    [Serializable]
    public class CombatAnimationOptions
    {
        [Tooltip("How much to affect the speed of an object")]
        public float SpeedMultiplier = 1.0f;

        [Tooltip("Whether action is synchronous or asynchronous.")]
        public bool WaitForCompletion = true;

        [Tooltip("Who is the subject of the animation.")]
        public CombatAnimationSubject Subject = CombatAnimationSubject.Source;

        public CombatTurnActionSummary Turn { get; set; }
        public CombatTurnUnitSummary FullTurn { get; set; }

        public CombatAnimationOptions Clone()
        {
            return new CombatAnimationOptions()
            {
                SpeedMultiplier = this.SpeedMultiplier,
                WaitForCompletion = this.WaitForCompletion,
                Subject = this.Subject,
                Turn = this.Turn,
                FullTurn = this.FullTurn,
            };
        }
    }
}
