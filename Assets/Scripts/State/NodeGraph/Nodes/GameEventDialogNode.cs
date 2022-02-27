using System.Collections;

namespace FlavBattle.State.NodeGraph.Nodes
{
    [CreateNodeMenu("GameEvent/Single/Dialog")]
    public class GameEventDialogNode : GameEventNodeSingleStepBase
    {
        protected override string NodeName => "Dialog";

        protected override IGameEventNodeGraphStep GetStepImpl(GameEventNodeGraphOptions options)
        {
            return new Runner(options, this);
        }

        private class Runner : GameEventNodeRunnerBase
        {
            private GameEventDialogNode _node;
            
            public Runner(GameEventNodeGraphOptions options, GameEventDialogNode node) : base(options)
            {
                _node = node;
            }

            public override IEnumerator Do()
            {
                // TODO
                yield return null;
            }
        }
    }
}
