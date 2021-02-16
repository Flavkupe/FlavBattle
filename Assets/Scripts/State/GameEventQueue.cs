using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.State
{
    public class GameEventQueue : MonoBehaviour
    {
        private Queue<IGameEvent> _eventQueue = new Queue<IGameEvent>();

        private KeyCode _cancelKey = KeyCode.Escape;

        private Coroutine _currentEvent = null;

        public event EventHandler AllDone;

        public bool Empty => _eventQueue.Count == 0 && _currentEvent == null;

        public void SetCancelKey(KeyCode key)
        {
            _cancelKey = key;
        }

        public void Update()
        {
            if (Input.GetKeyUp(_cancelKey))
            {
                CancelEvents();
            }

            if (_currentEvent == null && _eventQueue.Count > 0)
            {
                // No event running, but more queued; fetch next event
                NextEvent();
            }
        }

        public void CancelEvents()
        {
            if (Empty)
            {
                return;
            }

            if (_currentEvent != null)
            {
                StopCoroutine(_currentEvent);
            }

            _eventQueue.Clear();
            AllEventsDone();
        }

        private void AllEventsDone()
        {
            _currentEvent = null;
            AllDone?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Starts an event, or enqueues if an event is already running.
        /// </summary>
        public void AddOrStartEvent(IGameEvent e)
        {
            if (_currentEvent != null)
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
                NextEvent();
                return;
            }

            e.EventFinished += HandleEventFinished;
            _currentEvent = StartCoroutine(e.DoEvent());
        }

        private void HandleEventFinished(object sender, IGameEvent e)
        {
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
    }
}
