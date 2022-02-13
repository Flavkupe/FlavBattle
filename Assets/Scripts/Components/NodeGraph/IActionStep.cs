using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Components
{
    /// <summary>
    /// A full step of an action, such as a combat animation.
    /// </summary>
    public interface IActionStep
    {
        IEnumerator Do();
    }

    /// <summary>
    /// A full step of an action, such as a combat animation, with options
    /// </summary>
    public interface IActionNodeGraphStep<TOptions> : IActionStep where TOptions : ActionNodeGraphOptions
    {
        TOptions Options { get; }
    }

    public interface IActionNode<TOptions, TStep> where TOptions : ActionNodeGraphOptions where TStep : IActionNodeGraphStep<TOptions>
    {
        TStep GetStep(TOptions options);
    }

    /// <summary>
    /// Options object for a step that can be awaited.
    /// </summary>
    [Serializable]
    public class ActionNodeGraphOptions
    {
        [Tooltip("How much to affect the speed of an object")]
        public float SpeedMultiplier = 1.0f;

        [Tooltip("Whether action is synchronous or asynchronous.")]
        public bool WaitForCompletion = true;
    }
}
