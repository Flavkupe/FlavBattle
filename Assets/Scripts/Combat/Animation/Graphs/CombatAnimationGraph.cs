using FlavBattle.Combat.Animation.Nodes;
using FlavBattle.Components;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Graph", menuName = "Custom/Graphs/Combat Animation", order = 1)]
    public class CombatAnimationGraph : ActionNodeGraph<CombatTurnUnitSummary, CombatAnimationOptions>
    {
        [AssetIcon]
        public Sprite Icon;

        void OnEnable()
        {
            if (nodes.Count == 0)
            {
                var startNode = this.AddNode<FullAnimationNode>();
                startNode.name = "start";
            }
        }

        public override IActionNodeGraphStep<CombatAnimationOptions> GetStartStep(CombatTurnUnitSummary summary)
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
    }
}
