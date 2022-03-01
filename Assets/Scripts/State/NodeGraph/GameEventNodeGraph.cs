using FlavBattle.Components;
using FlavBattle.State.NodeGraph.Nodes;
using System;
using UnityEngine;

namespace FlavBattle.State.NodeGraph
{
    public class GaveEventNodeGraphInput
    {
        // TODO: require trigger as input

        public MonoBehaviour Initiator { get; set; }
    }

    [Serializable]
    public class GameEventNodeGraphOptions : ActionNodeGraphOptions
    {
        public GaveEventNodeGraphInput Input;

        public string Description;

        /// <summary>
        /// Whether the event can be skipped.
        /// </summary>
        [Tooltip("Whether the event can be skipped")]
        public bool Skippable = false;
    }

    [CreateAssetMenu(fileName = "Graph", menuName = "Custom/Graphs/Game Event", order = 1)]
    public class GameEventNodeGraph : ActionNodeGraph<GaveEventNodeGraphInput, GameEventNodeGraphOptions>
    {
        [AssetIcon]
        public Sprite Icon;

        void OnEnable()
        {
            if (nodes.Count == 0)
            {
                var startNode = this.AddNode<GameEventStartNode>();
                startNode.name = "start";
            }
        }

        public override IActionNodeGraphStep<GameEventNodeGraphOptions> GetStartStep(GaveEventNodeGraphInput input)
        {
            var options = new GameEventNodeGraphOptions()
            {
                Input = input,
            };

            var startNode = nodes.Find(a => a is GameEventStartNode) as GameEventStartNode;
            if (startNode == null)
            {
                Debug.LogError("Could not find starting node");
                return null;
            }

            return startNode.GetStep(options);
        }
    }
}
