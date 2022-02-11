using FlavBattle.Combat.Animation.Nodes;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Subgraph", menuName = "Custom/Graphs/Combat Animation Subgraph", order = 1)]
    public class CombatAnimationSubGraph : NodeGraph, ICombatAnimationNode
    {
        [AssetIcon]
        public Sprite Icon;

        void OnEnable()
        {
            if (nodes.Count == 0)
            {
                var startNode = this.AddNode<AnimationStartNode>();
                startNode.name = "start";
            }
        }

        public ICombatAnimationStep GetAnimation(CombatTurnUnitSummary summary)
        {
            var options = new CombatAnimationOptions()
            {
                FullTurn = summary,
            };

            var startNode = nodes.Find(a => a is FullAnimationNode) as FullAnimationNode;
            if (startNode == null)
            {
                Debug.LogError("Could not find starting node");
                return null;
            }

            return startNode.GetStep(options);
        }

        public ICombatAnimationStep GetStep(CombatAnimationOptions options)
        {
            var startNode = nodes.Find(a => a is AnimationStartNode) as AnimationStartNode;
            if (startNode == null)
            {
                Debug.LogError("Could not find starting node of substep");
                return null;
            }

            return startNode.GetStep(options);
        }
    }
}
