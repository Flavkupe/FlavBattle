using FlavBattle.Combat.Animation.Nodes;
using UnityEngine;
using XNode;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Graph", menuName = "Custom/Graphs/Combat Animation", order = 1)]
    public class CombatAnimationGraph : NodeGraph
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

        public ICombatAnimationStep GetAnimation(CombatTurnUnitSummary summary)
        {
            var options = new CombatAnimationOptions()
            {
                FullTurn = summary,
            };

            var startNode = nodes.Find(a => a is FullAnimationNode);
            if (startNode == null)
            {
                Debug.LogError("Could not find starting node");
                return null;
            }

            var fullNode = startNode as FullAnimationNode;
            if (fullNode == null)
            {
                Debug.LogError("Start node has wrong type.");
                return null;
            }

            return fullNode.GetStep(options);
        }
    }
}
