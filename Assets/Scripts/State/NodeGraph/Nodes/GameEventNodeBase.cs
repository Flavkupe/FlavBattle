using FlavBattle.Components;
using System.Collections;
using UnityEngine;
using XNode;

namespace FlavBattle.State.NodeGraph.Nodes
{
    public interface IGameEventNodeGraphStep : IActionNodeGraphStep<GameEventNodeGraphOptions> { }

    public interface IGameEventNode : IActionNode<GameEventNodeGraphOptions, IGameEventNodeGraphStep> {}

    public abstract class GameEventNodeBase : NodeBase, IGameEventNode
    {
        public abstract IGameEventNodeGraphStep GetStep(GameEventNodeGraphOptions options);
    }

    /// <summary>
    /// GameEventNode with options and a single Next and Previous step
    /// </summary>
    public abstract class GameEventNodeSingleStepBase : GameEventNodeBase
    {
        [Output]
        public GameEventNodeBase Next;

        [Input]
        public GameEventNodeBase Previous;

        [SerializeField]
        private GameEventNodeGraphOptions _options;

        public string Description;

        public override sealed IGameEventNodeGraphStep GetStep(GameEventNodeGraphOptions options)
        {
            _options.Input = options.Input;
            return GetStepImpl(_options);
        }

        protected abstract IGameEventNodeGraphStep GetStepImpl(GameEventNodeGraphOptions options);

        protected IGameEventNodeGraphStep GetNextStep(GameEventNodeGraphOptions options)
        {
            _options.Input = options.Input;
            var nextNode = this.GetOutputPortValue<GameEventNodeBase>(nameof(Next));
            return nextNode?.GetStep(options);
        }


        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Previous)) return GetInputValue(nameof(Previous), Previous);
            if (port.fieldName == nameof(Next)) return GetOutputPort(nameof(Next));
            else return base.GetValue(port);
        }
    }

    public abstract class GameEventNodeRunnerBase : IGameEventNodeGraphStep
    {
        public GameEventNodeGraphOptions Options { get; private set; }

        public GameEventNodeRunnerBase(GameEventNodeGraphOptions options)
        {
            Options = options;
        }

        public abstract IEnumerator Do();
    }
}
