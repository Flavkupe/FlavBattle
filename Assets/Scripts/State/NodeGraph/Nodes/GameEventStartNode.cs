using UnityEngine;
using XNode;

namespace FlavBattle.State.NodeGraph.Nodes
{
    [CreateNodeMenu("GameEvent/Special/Start (Default)")]
    public class GameEventStartNode : GameEventNodeBase
    {
        [Output]
        public GameEventNodeBase Start;

        protected override string NodeName => "Start";

        public override IGameEventNodeGraphStep GetStep(GameEventNodeGraphOptions options)
        {
            var nextNode = this.GetOutputPortValue<GameEventNodeBase>(nameof(Start));
            return nextNode?.GetStep(options);
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Start)) return GetOutputPort(nameof(Start));
            else return base.GetValue(port);
        }
    }
}
