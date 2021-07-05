using FlavBattle.Trace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.State
{
    public class GameEventQueue : MonoBehaviour, IHasTraceData
    {
        private Queue<IGameEvent> _eventQueue = new Queue<IGameEvent>();

        private KeyCode _cancelKey = KeyCode.Escape;

        private Coroutine _currentRoutine = null;
        private IGameEvent _currentEvent = null;

        public event EventHandler AllDone;

        public bool IsEmpty => _eventQueue.Count == 0 && _currentRoutine == null;

        public void SetCancelKey(KeyCode key)
        {
            _cancelKey = key;
        }

        void Update()
        {
            if (Input.GetKeyUp(_cancelKey))
            {
                CancelEvents();
            }

            if (_currentRoutine == null && _eventQueue.Count > 0)
            {
                // No event running, but more queued; fetch next event
                NextEvent();
            }
        }

        public void CancelEvents()
        {
            if (IsEmpty || _currentEvent == null)
            {
                return;
            }

            if (!_currentEvent.IsSkippable)
            {
                // Cannot skip
                return;
            }

            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
            }

            var followup = _currentEvent.TrySkipEvent();
            if (followup != null)
            {
                // This indicates that a subevent cannot be skipped
                _currentEvent = followup;
                StartEvent(followup);
                return;
            }

            _eventQueue.Clear();
            AllEventsDone();
        }

        private void AllEventsDone()
        {
            _currentRoutine = null;
            _currentEvent = null;
            AllDone?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Starts an event, or enqueues if an event is already running.
        /// </summary>
        public void AddOrStartEvent(IGameEvent e)
        {
            if (_currentRoutine != null)
            {
                _eventQueue.Enqueue(e);
            }
            else
            {
                StartEvent(e);
            }
        }

        private void StartEvent(IGameEvent e)
        {
            e.PreStartEvent();
            if (!e.EventPossible())
            {
                Logger.Trace(LogType.GameEvents, $"Event {e.EventName} not possible, skipping!");
                NextEvent();
                return;
            }

            e.EventFinished += HandleEventFinished;
            _currentEvent = e;

            Logger.Trace(LogType.GameEvents, $"Starting event {e.EventName}!");
            if (e.IsAsyncEvent)
            {
                // do event in background
                _currentRoutine = StartCoroutine(e.DoEvent());
            }
            else
            {
                // do event all at once and announce that it's complete
                _currentRoutine = StartCoroutine(DoEntireEvent(e));
            }
        }

        private IEnumerator DoEntireEvent(IGameEvent e)
        {
            yield return StartCoroutine(e.DoEvent());
            HandleEventFinished(e, e);
        }

        private void HandleEventFinished(object sender, IGameEvent e)
        {
            _currentRoutine = null;
            _currentEvent = null;
            if (e.FollowupEvent != null)
            {
                StartEvent(e.FollowupEvent);
            }
            else
            {
                NextEvent();
            }
        }

        private void NextEvent()
        {
            if (_eventQueue.Count > 0)
            {
                StartEvent(_eventQueue.Dequeue());
            }
            else
            {
                AllEventsDone();
            }
        }

        public TraceData GetTrace()
        {
            var data = TraceData.ChildTrace($"GameEventQueue ({this.name})",
                TraceData.ChildTrace("Current", _currentEvent?.EventName ?? "<None>")
            );

            return data;
        }
    }
}
