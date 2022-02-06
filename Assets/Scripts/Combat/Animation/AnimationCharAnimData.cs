using FlavBattle.Components;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Animator Trigger", order = 1)]
    public class AnimationCharAnimData : CombatAnimationData
    {
        public UnitAnimatorTrigger Animation;

        public TriggerInfo[] Followups;

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationCharAnim(this, options);
        }

        [Serializable]
        public class TriggerInfo
        {
            public UnitAnimatorEvent Event;
            public CombatAnimationDetails Animation;
        }
    }

    public class AnimationCharAnim : CombatAnimationActionStepBase<AnimationCharAnimData>
    {
        public AnimationCharAnim(AnimationCharAnimData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        private AnimatedCharacter _character;
        private CombatUnit _combatUnit;
        private int _runningExtraAnimations = 0;
       

        protected override IEnumerator DoAction()
        {
            PlayPreSounds();

            // find source position
            var subject = FullTurnSummary.Source;
            if (Options.Subject == CombatAnimationSubject.Target)
            {
                if (ActionSummary == null || ActionSummary.Target == null)
                {
                    Debug.LogWarning("Attempting targeted animation without target! Performing on self.");
                }
                else
                {
                    subject = ActionSummary.Target;
                }
            }

            _combatUnit = subject.CombatUnit;
            _character = _combatUnit.Character;

            if (Options.WaitForCompletion)
            {
                yield return Animate();
            }
            else
            {
                _character.StartCoroutine(Animate());
            }

            PlayPostSounds();
        }

        private IEnumerator Animate()
        {
            // ensure handler is only added once
            _character.AnimationEvent -= HandleAnimationEvent;
            _character.AnimationEvent += HandleAnimationEvent;

            yield return _combatUnit.PlayAnimatorToCompletion(Data.Animation);

            // always remove event
            _character.AnimationEvent -= HandleAnimationEvent;

            while (_runningExtraAnimations > 0)
            {
                // wait for extra animations to finish
                yield return null;
            }
        }

        private void HandleAnimationEvent(object sender, UnitAnimatorEvent e)
        {
            if (_character == null)
            {
                return;
            }

            var followups = Data.Followups.Where(a => a.Event == e);
            foreach (var item in followups)
            {
                var anim = item.Animation.Create(Options);
                _character.StartCoroutine(DoExtraAnimation(anim));
            }

            _character.AnimationEvent -= HandleAnimationEvent;
        }

        private IEnumerator DoExtraAnimation(ICombatAnimationStep anim)
        {
            _runningExtraAnimations++;
            yield return PerformAction(anim.Do());
            _runningExtraAnimations--;
        }
    }
}
