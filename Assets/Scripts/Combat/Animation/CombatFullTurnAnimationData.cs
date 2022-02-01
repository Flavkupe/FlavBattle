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

    public class CombatFullTurnAnimation
    {
        private CombatTurnUnitSummary _summary;
        private CombatFullTurnAnimationData _data;
        public CombatFullTurnAnimation(CombatFullTurnAnimationData data, CombatTurnUnitSummary summary)
        {
            _summary = summary;
            _data = data;
        }

        public IEnumerator Do()
        {
            var options = new CombatAnimationOptions()
            {
                FullTurn = _summary,
            };

            foreach (var stepData in _data.PreAction)
            {
                var step = stepData.Create(options);
                yield return step.Do();
            }

            foreach (var turn in _summary.Results)
            {
                foreach (var stepData in _data.Action)
                {
                    options.Turn = turn;
                    var step = stepData.Create(options);
                    yield return step.Do();
                }
            }

            foreach (var stepData in _data.PostAction)
            {
                var step = stepData.Create(options);
                yield return step.Do();
            }
        }
    }
}
