using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Combat Animation Set", order = 1)]
    public class CombatAnimationSetData : CombatAnimationData
    {
        [SerializeField]
        private CombatAnimationDetails[] _animations;
        public CombatAnimationDetails[] Animations => _animations;

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new CombatAnimationSet(this, options);
        }
    }

    public class CombatAnimationSet : CombatAnimationStepBase<CombatAnimationSetData>
    {
        private CombatAnimationOptions _options;

        public CombatAnimationSet(CombatAnimationSetData data, CombatAnimationOptions options) : base(data, options)
        {
            _options = options;
        }

        public override IEnumerator Do()
        {
            foreach (var item in Data.Animations)
            {
                var anim = item.Create(_options);
                yield return PerformAction(anim.Do());
            }
        }
    }
}
