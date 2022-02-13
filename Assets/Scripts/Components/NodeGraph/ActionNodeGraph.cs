using XNode;

namespace FlavBattle.Components
{
    /// <summary>
    /// A NodeGraph with nodes that perform Coroutine actions and can
    /// be awaited.
    /// </summary>
    /// <typeparam name="TInput">Input object to initialize this graph (eg, combat summary).</typeparam>
    /// <typeparam name="TOptions">Type of options for individual steps.</typeparam>
    public abstract class ActionNodeGraph<TInput, TOptions> : NodeGraph where TOptions : ActionNodeGraphOptions
    {
        public abstract IActionNodeGraphStep<TOptions> GetStartStep(TInput input);
    }
}