using System;
using System.Collections;
using System.Collections.Generic;

public class RoutineChain : IEnumerator, IRoutineSet
{
    private Queue<Routine> _queue = new Queue<Routine>();

    private Action _then = null;

    private Routine _current = null;

    public RoutineChain()
    {
    }

    public Routine AsRoutine()
    {
        return Routine.Create(() => this);
    }

    public RoutineChain(params Routine[] routines)
    {
        foreach (Routine routine in routines)
        {
            _queue.Enqueue(routine);
        }
    }

    public void AddAction(Action action)
    {
        _queue.Enqueue(Routine.CreateAction(action));
    }

    public void AddRoutine(Routine routine)
    {
        _queue.Enqueue(routine);
    }

    public void Then(Action action)
    {
        _then = action;
    }

    public object Current { get { return _current; } }

    public bool MoveNext()
    {
        if (_queue.Count == 0)
        {
            _current = null;
            if (_then != null)
            {
                _then();
            }

            return false;
        }

        _current = _queue.Dequeue();
        return true;
    }

    public void Reset()
    {
    }
}
