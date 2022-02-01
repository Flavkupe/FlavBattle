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

        AnimatedCharacter _character;

        public override IEnumerator Do()
        {
            // find source position
            var subject = Options.Subject == CombatAnimationSubject.Source ? ActionSummary.Source : ActionSummary.Target;
            var combatUnit = subject.CombatUnit;
            _character = combatUnit.Character;

            // ensure handler is only added once
            _character.AnimationEvent -= HandleAnimationEvent;
            _character.AnimationEvent += HandleAnimationEvent;

            if (Options.WaitForCompletion)
            {
                yield return combatUnit.PlayAnimatorToCompletion(Data.Animation);
            }
            else
            {
                combatUnit.PlayAnimator(Data.Animation);
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
                _character.StartCoroutine(anim.Do());
            }

            _character.AnimationEvent -= HandleAnimationEvent;
        }
    }
}
