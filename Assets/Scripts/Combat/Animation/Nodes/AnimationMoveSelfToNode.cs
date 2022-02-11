using FlavBattle.Combat.Animation;
using FlavBattle.Combat.Animation.Nodes;

namespace Assets.Scripts.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Single/Move")]
    public class AnimationMoveSelfToNode : SingleCombatAnimationDataNode<AnimationMoveSelfToData>
    {
        protected override string NodeName => "Move";
    }
}
