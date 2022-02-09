using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Combat Full Turn Animation", order = 1)]
    public class CombatFullTurnAnimationData : ScriptableObject
    {
        [AssetIcon]
        public Sprite Icon;

        [SerializeField]
        private CombatAnimationDetails[] _preAction;
        public CombatAnimationDetails[] PreAction => _preAction;

        [SerializeField]
        private CombatAnimationDetails[] _action;
        public CombatAnimationDetails[] Action => _action;

        [SerializeField]
        private CombatAnimationDetails[] _postAction;
        public CombatAnimationDetails[] PostAction => _postAction;

        public CombatFullTurnAnimation Create(CombatTurnUnitSummary summary)
        {
            return new CombatFullTurnAnimation(this, summary);
        }
    }

    public class CombatFullTurnAnimation : ICombatAnimationStep
    {
        private CombatTurnUnitSummary _summary;
        private CombatFullTurnAnimationData _data;
        private CombatAnimationOptions _options;

        public CombatFullTurnAnimation(CombatFullTurnAnimationData data, CombatTurnUnitSummary summary)
        {
            _summary = summary;
            _data = data;
            _options = new CombatAnimationOptions()
            {
                FullTurn = _summary,
            };
        }

        public CombatAnimationOptions Options => _options;

        public IEnumerator Do()
        {
            foreach (var stepData in _data.PreAction)
            {
                var step = stepData.Create(_options);
                yield return PerformAction(step, step.Options.WaitForCompletion);
            }

            foreach (var turn in _summary.Results)
            {
                foreach (var stepData in _data.Action)
                {
                    _options.Turn = turn;
                    var step = stepData.Create(_options);
                    yield return PerformAction(step, step.Options.WaitForCompletion);
                }
            }

            foreach (var stepData in _data.PostAction)
            {
                var step = stepData.Create(_options);
                yield return PerformAction(step, step.Options.WaitForCompletion);
            }
        }

        private IEnumerator PerformAction(ICombatAnimationStep step, bool waitForCompletion)
        {
            
            if (waitForCompletion)
            {
                yield return step.Do();
            }
            else
            {
                var actor = _summary.Source.CombatUnit;
                actor.StartCoroutine(step.Do());
            }
        }
    }
}
