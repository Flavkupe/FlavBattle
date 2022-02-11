
using FlavBattle.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Single/Character")]
    public class AnimationCharAnimNode : SingleAnimationNode
    {
        protected override string NodeName => "Character";

        public UnitAnimatorTrigger Animation;

        [Tooltip("Options for the Animation, and whether to wait for that before continuing to Next")]
        public CombatAnimationOptions Options;

        public AnimationRunningBehavior RunningBehavior;

        [Output(dynamicPortList = true)]
        [SerializeField]
        private EventInfo[] EventHandlers;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Animation)) return GetInputPort(nameof(Animation));
            if (port.fieldName == nameof(EventHandlers)) return GetInputPort(nameof(EventHandlers));
            else return base.GetValue(port);
        }

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            var eventNodes = new List<EventNode>();

            for (var i = 0; i < EventHandlers.Length; i++)
            {
                var eventInfo = EventHandlers[i];
                var node = this.GetOutputPortValue<AnimationNodeBase>($"{nameof(EventHandlers)} {i}");
                var eventNode = new EventNode()
                {
                    Event = eventInfo.Event,
                    Await = eventInfo.Await,
                    Node = node,
                };
                eventNodes.Add(eventNode);
            }

            var opts = Options.Clone(options.FullTurn, options.Turn);
            return new Runner(opts, Animation, RunningBehavior, eventNodes);
        }

        [Serializable]
        private class EventInfo
        {
            public UnitAnimatorEvent Event;
            public bool Await;
        }

        [Serializable]
        private class EventNode : EventInfo
        {
            public AnimationNodeBase Node;
        }

        /// <summary>
        /// What to do if animation is already running
        /// </summary>
        public enum AnimationRunningBehavior
        {
            /// <summary>
            /// Wait for animation to finish and repeat it
            /// </summary>
            WaitAndRepeat,

            /// <summary>
            /// Does not animate, and continues the next node
            /// </summary>
            IgnoreAndContinue,

            /// <summary>
            /// Repeats the animation without waiting for the first to finish.
            /// WARNING: This should not be used if there are expected EventHandlers.
            /// </summary>
            RepeatWithoutWaiting,
        }

        private class Runner : AnimationStepRunnerBase
        {
            private AnimatedCharacter _character;
            private CombatUnit _combatUnit;
            private int _runningExtraAnimations = 0;
            private List<EventNode> _eventNodes;
            private UnitAnimatorTrigger _animation;
            private AnimationRunningBehavior _runningBehavior;

            public Runner(CombatAnimationOptions options,
                UnitAnimatorTrigger animation,
                AnimationRunningBehavior runningBehavior,
                List<EventNode> eventNodes) : base(options)
            {
                _eventNodes = eventNodes;
                _animation = animation;
                _runningBehavior = runningBehavior;
            }

            public override IEnumerator Do()
            {
                // find source position
                var subject = Options.FullTurn.Source;
                if (Options.Subject == CombatAnimationSubject.Target)
                {
                    if (Options.Turn == null || Options.Turn.Target == null)
                    {
                        Debug.LogWarning("Attempting targeted animation without target! Performing on self.");
                    }
                    else
                    {
                        subject = Options.Turn.Target;
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
            }

            private IEnumerator Animate()
            {
                var state = _animation.GetStateFromTrigger();

                if (_combatUnit.IsInAnimationState(state))
                {
                    if (_runningBehavior == AnimationRunningBehavior.IgnoreAndContinue)
                    {
                        yield break;
                    }
                    else if (_runningBehavior == AnimationRunningBehavior.WaitAndRepeat)
                    {

                        // IMPORTANT: if an animation is already running, let it finish first before continuing;
                        // the character can only have a single animation at a time, so skipping the animation will
                        // otherwise lead to skipping important animation events.
                        yield return _combatUnit.WaitForAnimationEnd(state);
                    }
                }

                _character.AnimationEvent += HandleAnimationEvent;

                yield return _combatUnit.PlayAnimatorToCompletion(_animation, Options.SpeedMultiplier);

                _character.AnimationEvent -= HandleAnimationEvent;

                while (_runningExtraAnimations > 0)
                {
                    // wait for extra animations to finish
                    yield return null;
                }
            }

            private void HandleAnimationEvent(object o, UnitAnimatorEvent e)
            {
                if (_character == null)
                {
                    return;
                }

                // unregister as soon as event is handled to ensure we don't duolicate the handling of the event
                // before the rest of the animation completes
                _character.AnimationEvent -= HandleAnimationEvent;

                var followups = _eventNodes.Where(a => a.Event == e);
                foreach (var item in followups)
                {
                    var anim = item.Node.GetStep(Options);
                    _character.StartCoroutine(DoExtraAnimation(anim));
                }
            }

            private IEnumerator DoExtraAnimation(ICombatAnimationStep anim)
            {
                _runningExtraAnimations++;
                yield return anim.Do().PerformAction(Options);
                _runningExtraAnimations--;
            }
        }
    }
}
