
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoutineConvertable
{
    Routine AsRoutine();
}

public interface IRoutineSet : IRoutineConvertable, IEnumerator
{
    void AddRoutine(Routine routine);
}

public class Routine : IEnumerator
{
    private Func<IEnumerator> _func;
    private Routine _next = null;
    private IEnumerator _current = null;
    private bool _executedFinallies = false;

    /// <summary>
    /// Behavior that happens if an exception is thrown
    /// </summary>
    private List<Action<Exception>> _catch = new List<Action<Exception>>();

    public List<Action<Exception>> CatchHandlers
    {
        get { return _catch; }
    }

    private List<Action> _reject = new List<Action>();

    protected bool _rejected = false;

    private List<Action> _finally = new List<Action>();

    private List<Action> _completed = new List<Action>();

    private bool _executed = false;

    // For debug only
    public string Trace = null;

    protected Routine()
    {
    }

    public Action CancellationCallback
    {
        get { return () => _rejected = true; }
    }

    public Routine(Func<IEnumerator> func)
        : this()
    {       
        _func = func;
    }

    public bool DisableSafeMode = false;

    public object Current
    {
        get
        {
            IEnumerator enumerator = _current ?? (_next != null ? (IEnumerator)_next.Current : null);
            return DisableSafeMode ? enumerator : RunSafe(enumerator);
        }
    }

    protected virtual IEnumerator Execute()
    {
        return _func();
    } 

    protected virtual bool ShouldInterrupt()
    {
        return false;
    }

    public bool MoveNext()
    {
        try
        {
            if (ShouldInterrupt())
            {
                _rejected = true;
            }

            if (_rejected)
            {
                foreach (var doReject in _reject)
                {
                    doReject();
                }

                RunFinallies();

                return false;
            }

            if (!_executed)
            {
                _executed = true;
                _current = Execute();
                if (_current != null)
                {
                    return true;
                }
            }
            else
            {
                _current = null;
            }

            if (_next != null)
            {
                if (_next._catch.Count == 0)
                {
                    // Inherit catch block if not overriden.
                    _next._catch = _catch;
                }

                if (_next.MoveNext())
                {
                    return true;
                }
            }

            foreach (var doCompleted in _completed)
            {
                doCompleted();
            }

            RunFinallies();

            return false;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return false;
        }
    }

    private void RunFinallies()
    {
        if (_executedFinallies)
        {
            return;
        }

        _executedFinallies = true;

        foreach (var doFinally in _finally)
        {
            try
            {
                // try/catch the finally individually to avoid running finallies
                // twice if exceptions are thrown
                doFinally();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }        
    }

    public void Reset()
    {
        // TODO
    }

    private void HandleException(Exception ex)
    {
        // Catch any exceptions and run the finally blocks
        Debug.LogError("Caught exception in Routine!");
        Debug.LogError(ex);

        RunFinallies();

        foreach (var doCatch in _catch)
        {
            try
            {
                doCatch(ex);
            }
            catch (Exception ex2)
            {
                Debug.LogError(ex2);
            }
        }
    }

    private IEnumerator RunSafe(IEnumerator enumerator)
    {
        if (enumerator == null)
        {
            yield break;
        }

        while (true)
        {
            object current;
            try
            {
                if (enumerator.MoveNext() == false)
                {
                    break;
                }

                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                yield break;
            }

            yield return current;
        }
    }

    public IEnumerator RunSafe()
    {
        yield return RunSafe(this);
    }

    public IEnumerator RunSafe(Routine routine)
    {
        IEnumerator enumerator = routine.Execute();
        yield return RunSafe(enumerator);
    }

    /// <summary>
    /// For when an exception is thrown from within
    /// </summary>
    public Routine Catch(Action<Exception> action)
    {
        _catch.Add(action);
        return this;
    }

    public void OnReject(Action action)
    {
        _reject.Add(action);
    }

    public void OnCompleted(Action action)
    {
        _completed.Add(action);
    }

    /// <summary>
    /// Runs at the end, no matter what (even if rejected)
    /// </summary>
    /// <param name="action"></param>
    /// <param name="addToFront">Set to true if this action should take priority over all other 'finally' actions.</param>
    public void Finally(Action action, bool addToFront = false)
    {
        if (addToFront)
        {
            _finally.Insert(0, action);
        }
        else
        {
            _finally.Add(action);
        }
    }

    /// <summary>
    /// Queues an Action callback to happen after the Routine completes.
    /// If a Routine is also hooked via Then, this Action will happen first.
    /// Returns this object for chaining.
    /// </summary>
    public Routine Then(Action action)
    {
        return Then(Routine.CreateAction(action));
    }

    public Routine Then(Func<IEnumerator> func)
    {
        return Then(Routine.Create(func));
    }

    /// <summary>
    /// Queues a Routine to happen after this Routine completes. Only one of these
    /// can be set per Routine, but they can be chained by calling this function on the returned
    /// Routine reference. If a "Then" has already been set for this Routine, the next routine
    /// with be appended to the end of the Routine chain.
    /// </summary>
    /// <param name="next">Which Routine to run after this Routine completes.</param>
    /// <returns>A reference to the provided parameter, to ease chaining.</returns>
    public Routine Then(Routine next)
    {
        if (_next == null)
        {
            _next = next;
            return _next;
        }
        else
        {
            return _next.Then(next);
        }
    }

    private static IEnumerator DoActionQuick(Action action)
    {
        action();
        yield break;
    }

    private static IEnumerator DoActionQuick<T>(Action<T> action, T arg1)
    {
        action(arg1);
        yield break;
    }

    /// <summary>
    /// Big dumb empty routine, does nothing
    /// </summary>
    public static Routine Empty
    {
        get { return CreateAction(() => {}); }
    }

    public static Routine CreateAction(Action action)
    {
        return Routine.Create(() => DoActionQuick(action));
    }

    public static Routine CreateAction<T>(Action<T> action, T arg1)
    {
        return Routine.Create(() => DoActionQuick(action, arg1));
    }

    public static Routine Create(IEnumerator coroutine)
    {
        return Create(() => coroutine);
    }

    public static Routine Create(Func<IEnumerator> func)
    {
        return new Routine(func);
    }

    public static Routine<T> Create<T>(Func<T, IEnumerator> func, T arg1)
    {
        return new Routine<T>(func, arg1);
    }

    public static Routine<T1, T2> Create<T1, T2>(Func<T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
    {
        return new Routine<T1, T2>(func, arg1, arg2);
    }

    public static Routine<T1, T2, T3> Create<T1, T2, T3>(Func<T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
    {
        return new Routine<T1, T2, T3>(func, arg1, arg2, arg3);
    }

    public static Routine<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return new Routine<T1, T2, T3, T4>(func, arg1, arg2, arg3, arg4);
    }

    public static CancellableRoutine CreateCancellable(Func<Action, IEnumerator> func, KeyCode cancelKey)
    {
        return new CancellableRoutine(func, cancelKey);
    }

    public static CancellableRoutine CreateCancellable(Func<Action, IEnumerator> func)
    {
        return new CancellableRoutine(func);
    }

    public static CancellableRoutine<T> CreateCancellable<T>(Func<Action, T, IEnumerator> func, T arg1)
    {
        return new CancellableRoutine<T>(func, arg1);
    }

    public static CancellableRoutine<T1, T2> CreateCancellable<T1, T2>(Func<Action, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
    {
        return new CancellableRoutine<T1, T2>(func, arg1, arg2);
    }

    public static IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Creates a routine set that runs sequentially or in parallel, depending on second param.
    /// </summary>
    /// <param name="runner">Something that can run StartCoroutine.</param>
    /// <param name="parallel">Whether to run routines in parallel or not.</param>
    /// <param name="staggerTime">Parallel is staggered by staggerTime if third param is provided. No effect on  sequential.</param>
    /// <returns></returns>
    public static IRoutineSet CreateEmptyRoutineSet(MonoBehaviour runner, bool parallel, float staggerTime = 0.0f)
    {
        if (parallel)
        {
            var set = new ParallelRoutineSet(runner);
            set.StaggerTime = staggerTime;
            return set;
        }
        else
        {
            return new RoutineChain();
        }
    }
}

public class Routine<T> : Routine
{
    private Func<T, IEnumerator> _func;
    protected T _arg1;

    public Routine(Func<T, IEnumerator> func, T arg1)
        : base()
    {
        _func = func;
        _arg1 = arg1;
    }

    protected override IEnumerator Execute()
    {
        return _func(_arg1);
    }
}

public class Routine<T1, T2> : Routine
{
    private Func<T1, T2, IEnumerator> _func;
    protected T1 _arg1;
    protected T2 _arg2;

    public Routine(Func<T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        : base()
    {
        _func = func;
        _arg1 = arg1;
        _arg2 = arg2;
    }

    protected override IEnumerator Execute()
    {
        return _func(_arg1, _arg2);
    }
}

public class Routine<T1, T2, T3> : Routine
{
    private Func<T1, T2, T3, IEnumerator> _func;
    protected T1 _arg1;
    protected T2 _arg2;
    protected T3 _arg3;

    public Routine(Func<T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        : base()
    {
        _func = func;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
    }

    protected override IEnumerator Execute()
    {
        return _func(_arg1, _arg2, _arg3);
    }
}

public class Routine<T1, T2, T3, T4> : Routine
{
    private Func<T1, T2, T3, T4, IEnumerator> _func;
    protected T1 _arg1;
    protected T2 _arg2;
    protected T3 _arg3;
    protected T4 _arg4;

    public Routine(Func<T1, T2, T3, T4, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        : base()
    {
        _func = func;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
    }

    protected override IEnumerator Execute()
    {
        return _func(_arg1, _arg2, _arg3, _arg4);
    }
}

public class CancellableRoutine : Routine<Action>
{
    private KeyCode? _cancelKey = null;

    protected override bool ShouldInterrupt()
    {
        if (_cancelKey == null)
        {
            return false;
        }

        return Input.GetKey(_cancelKey.Value);
    }

    public CancellableRoutine(Func<Action, IEnumerator> func, KeyCode? cancelKey = null)
        : base(func, null)
    {
        _arg1 = CancellationCallback;
        _cancelKey = cancelKey;
    }
}

public class CancellableRoutine<T> : Routine<Action, T>
{
    public CancellableRoutine(Func<Action, T, IEnumerator> func, T arg1)
        : base(func, null, arg1)
    {
        _arg1 = CancellationCallback;
    }
}

public class CancellableRoutine<T1, T2> : Routine<Action, T1, T2>
{
    public CancellableRoutine(Func<Action, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        : base(func, null, arg1, arg2)
    {
        _arg1 = CancellationCallback;
    }
}

public static class RoutineExtensionFunctions
{
    public static Routine ToRoutine(this IEnumerator enumerator)
    {
        if (enumerator == null)
        {
            Debug.LogWarning("Converting empty routine!");
            return Routine.Empty;
        }

        return Routine.Create(enumerator);
    }

    public static IEnumerator RunInBackground(this Routine routine, MonoBehaviour runner)
    {
        yield return runner.StartCoroutine(routine);
    }
}
