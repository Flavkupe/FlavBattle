using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Combat.Animation.Nodes
{
    [CreateNodeMenu("Animation/Subgraph")]
    public class AnimationSubgraphNode : SingleAnimationNode<CombatAnimationSubGraph>
    {
        protected override string NodeName => "Subgraph";

        [ShowAssetPreview(32, 32)]
        [SerializeField]
        private Sprite _icon;

        protected override ICombatAnimationStep GetAnimationStep(CombatAnimationOptions options)
        {
            return Data.GetStep(options);
        }

        private void OnValidate()
        {
            if (Data != null)
            {
                name = Data.name;
                _icon = Data.Icon;
            }
        }
    }
}
