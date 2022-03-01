using FlavBattle.Components;
using UnityEngine;
using XNode;

namespace FlavBattle.State.NodeGraph.Nodes
{
    public abstract class GameEventInputNodeBase<TValue> : NodeBase
    {
        [Output]
        public GameEventNodeBase Consumer;

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Consumer)) return this.GetValue();
            else return base.GetValue(port);
        }

        public abstract TValue GetValue();
    }

    /// <summary>
    /// A GameEvent graph input node that returns a GameObject
    /// </summary>
    [NodeTint("#00AA33")]
    public abstract class GameEventInputGameObjectNodeBase : GameEventInputNodeBase<GameObject>
    {
    }
}
