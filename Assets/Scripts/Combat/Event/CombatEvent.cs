using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Group of combat events to be processed
/// </summary>
public class CombatAnimationEventSequence : ICombatAnimationEvent
{
    public List<ICombatAnimationEvent> Events { get; private set; } = new List<ICombatAnimationEvent>();

    /// <summary>
    /// Whether the events should happen in parallel. Defaults to true;
    /// </summary>
    public bool Parallel { get; set; } = true;

    /// <summary>
    /// Optional time to stagger animations for multiple events, giving some time between
    /// parallel events. Defaults to 0.0f (no stagger).
    /// </summary>
    public float StaggerTime { get; set; } = 0.0f;

    private MonoBehaviour _owner;

    public CombatAnimationEventSequence(MonoBehaviour owner, List<ICombatAnimationEvent> events)
    {
        Events = events;
        _owner = owner;
    }

    public CombatAnimationEventSequence(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void AddEvent(ICombatAnimationEvent newEvent)
    {
        Events.Add(newEvent);
    }

    public IEnumerator Animate()
    {
        var routines = Routine.CreateEmptyRoutineSet(_owner, Parallel, StaggerTime);
        foreach (var item in Events)
        {
            var routine = Routine.Create(item.Animate());
            routines.AddRoutine(routine);
        }

        yield return routines;
    }
}

/// <summary>
/// An event that performs some sort of animation that can be awaited.
/// </summary>
public interface ICombatAnimationEvent
{
    IEnumerator Animate();
}
