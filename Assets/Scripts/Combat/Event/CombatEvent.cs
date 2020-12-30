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
        var routines = Routine.CreateEmptyRoutineSet(_owner, Parallel);
        foreach (var item in Events)
        {
            routines.AddRoutine(Routine.Create(item.Animate()));
        }
        
        yield return routines;
    }
}

public class CombatProcessEventSequence<TResultType> : ICombatProcessEvent<List<TResultType>>
{
    public List<ICombatProcessEvent<TResultType>> Events { get; private set; } = new List<ICombatProcessEvent<TResultType>>();

    public CombatProcessEventSequence(List<ICombatProcessEvent<TResultType>> events)
    {
        Events = events;
    }

    public List<TResultType> Process()
    {
        List<TResultType> results = new List<TResultType>();
        foreach (var item in Events)
        {
            results.Add(item.Process());
        }

        return results;
    }
}

public interface ICombatAnimationEvent
{
    IEnumerator Animate();
}

public interface ICombatProcessEvent<TResultType>
{
    TResultType Process();
}
