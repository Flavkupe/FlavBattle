using FlavBattle.Components;
using FlavBattle.Utils;
using System.Collections;
using UnityEngine;
using XNode;

namespace FlavBattle.State.NodeGraph.Nodes
{
    public interface IGameEventNodeGraphStep : IActionNodeGraphStep<GameEventNodeGraphOptions> {
        IEnumerator OnSkipped();
    }

    public interface IGameEventNode : IActionNode<GameEventNodeGraphOptions, IGameEventNodeGraphStep> {}

    public abstract class GameEventNodeBase : NodeBase, IGameEventNode
    {
        public abstract IGameEventNodeGraphStep GetStep(GameEventNodeGraphOptions options);
    }

    /// <summary>
    /// GameEventNode with options and a single Next and Previous step,
    /// and the ability to skip a step
    /// </summary>
    public abstract class GameEventNodeSingleStepBase : GameEventNodeBase
    {
        [Output]
        public GameEventNodeBase OnSkip;

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
            var currentStep = GetStepImpl(_options);
            var nextNode = this.GetOutputPortValue<GameEventNodeBase>(nameof(Next));
            var nextStep = nextNode?.GetStep(_options);

            var skipNode = this.GetOutputPortValue<GameEventNodeBase>(nameof(OnSkip));
            var skipStep = skipNode?.GetStep(_options);
            return new Runner(_options, currentStep, nextStep, skipStep);
        }

        protected abstract IGameEventNodeGraphStep GetStepImpl(GameEventNodeGraphOptions options);

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == nameof(Previous)) return GetInputValue(nameof(Previous), Previous);
            if (port.fieldName == nameof(Next)) return GetOutputPort(nameof(Next));
            if (port.fieldName == nameof(OnSkip)) return GetOutputPort(nameof(OnSkip));
            else return base.GetValue(port);
        }

        private class Runner : GameEventNodeRunnerBase
        {
            private IGameEventNodeGraphStep _nextStep;
            private IGameEventNodeGraphStep _currentStep;
            private IGameEventNodeGraphStep _skipStep;

            public Runner(GameEventNodeGraphOptions options,
                IGameEventNodeGraphStep currentStep,
                IGameEventNodeGraphStep nextStep,
                IGameEventNodeGraphStep skipStep) : base(options)
            {
                _currentStep = currentStep;
                _nextStep = nextStep;
                _skipStep = skipStep;
            }

            public override IEnumerator Do()
            {
                var skipped = false;
                if (_currentStep != null)
                {
                    var routine = _currentStep.Do();
                    if (Options.Skippable)
                    {
                        var runner = new CancellableCoroutineContainer(Options.Input.Initiator, routine);
                        yield return runner.Do();
                        skipped = runner.Skipped;
                        if (skipped)
                        {
                            yield return _currentStep.OnSkipped();
                        }
                    }
                    else
                    {
                        yield return routine;
                    }
                }

                if (_skipStep != null && skipped)
                {
                    yield return _skipStep.Do();
                }
                else if (_nextStep != null)
                {
                    yield return _nextStep.Do();
                }
            }
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

        public virtual IEnumerator OnSkipped()
        {
            yield return null;
        }
    }
}
