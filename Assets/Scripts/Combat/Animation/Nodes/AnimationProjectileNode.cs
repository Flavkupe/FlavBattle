

using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Single/Projectile")]
    public class AnimationProjectileNode : SingleCombatAnimationDataNode<AnimationProjectileData>
    {
        protected override string NodeName => "Projectile";
    }
}
