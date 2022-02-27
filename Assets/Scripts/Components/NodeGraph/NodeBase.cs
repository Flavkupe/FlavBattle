using XNode;

namespace FlavBattle.Components
{
    public abstract class NodeBase : Node
    {
        protected abstract string NodeName { get; }

        protected virtual void OnReset()
        {
        }

        private void Reset()
        {
            name = NodeName;
            OnReset();
        }
    }
}
