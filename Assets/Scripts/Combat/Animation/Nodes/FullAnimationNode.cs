using FlavBattle.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Special/Full Animation (Default)")]
    public class FullAnimationNode : Node, ICombatAnimationNode
    {
        [SerializeField]
        [Output]
        private AnimationNodeBase _preActions;

        [SerializeField]
        [Output]
        private AnimationNodeBase _actions;

        [SerializeField]
        [Output]
        private AnimationNodeBase _postActions;

        public ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            return new CombatFullTurnAnimationSteps(this, options);
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(_preActions)) return GetOutputPort(nameof(_preActions));
            if (port.fieldName == nameof(_actions)) return GetOutputPort(nameof(_actions));
            if (port.fieldName == nameof(_postActions)) return GetOutputPort(nameof(_postActions));
            else return null;
        }

        private AnimationNodeBase GetAction(string fieldName)
        {
            return this.GetOutputPortValue<AnimationNodeBase>(fieldName);
        }

        public AnimationNodeBase PreActions => GetAction(nameof(_preActions));
        public AnimationNodeBase Actions => GetAction(nameof(_actions));
        public AnimationNodeBase PostActions => GetAction(nameof(_postActions));
    }

    public class CombatFullTurnAnimationSteps : ICombatAnimationStep
    {
        private CombatAnimationOptions _options;
        private FullAnimationNode _data;

        public CombatFullTurnAnimationSteps(FullAnimationNode data, CombatAnimationOptions options)
        {
            _options = options;
            _data = data;
        }

        public CombatAnimationOptions Options => _options;

        public IEnumerator Do()
        {
            if (_data.PreActions != null)
            {
                var step = _data.PreActions.GetStep(_options);
                yield return step.Do();
            }

            if (_data.Actions != null)
            {
                var step = _data.Actions.GetStep(_options);
                yield return step.Do();
            }

            if (_data.PostActions != null)
            {
                var step = _data.PostActions.GetStep(_options);
                yield return step.Do();
            }
        }
    }
}
