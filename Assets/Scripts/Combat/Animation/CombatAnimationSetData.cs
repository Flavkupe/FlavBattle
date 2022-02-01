using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Combat Animation Set", order = 1)]
    public class CombatAnimationSetData : CombatAnimationData
    {
        [SerializeField]
        private CombatAnimationData[] _animations;
        public CombatAnimationData[] Animations => _animations;

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new CombatAnimationSet(this, options);
        }
    }

    public class CombatAnimationSet : CombatAnimationStepBase<CombatAnimationSetData>
    {
        private CombatAnimationOptions _options;

        public CombatAnimationSet(CombatAnimationSetData data, CombatAnimationOptions options) : base(data)
        {
            _options = options;
        }

        public override IEnumerator Do()
        {
            foreach (var item in Data.Animations)
            {
                var anim = item.Create(_options);
                if (_options.WaitForCompletion)
                {
                    yield return anim.Do();
                }
                else
                {
                    // Routine.Create(anim.Do()).RunInBackground();
                }
            }
        }
    }
}
