using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.State
{
    public interface IGameEvent
    {
        bool EventPossible();
        void TriggerEvent();

        /// <summary>
        /// Called right before attempting to start an event, or
        /// checking if it's possible. Initializes all values from
        /// the moment the event is about to start.
        /// </summary>
        void PreStartEvent();

        /// <summary>
        /// The next event that should happen (or null if this is the last one)
        /// </summary>
        IGameEvent FollowupEvent { get; }

        /// <summary>
        /// Runs through the event. Can be interrupted.
        /// </summary>
        /// <returns></returns>
        IEnumerator DoEvent();

        /// <summary>
        /// Fires when the event completes.
        /// </summary>
        event EventHandler<IGameEvent> EventTriggered;

        /// <summary>
        /// Fires when the event completes.
        /// </summary>
        event EventHandler<IGameEvent> EventFinished;

        /// <summary>
        /// Whether the event has been triggered.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Marks event to be cancelled. Sets Triggered to true.
        /// </summary>
        void CancelEvent();
    }

    public abstract class GameEventBase : MonoBehaviour, IGameEvent
    {
        /// <summary>
        /// Fires when this event is ready to invoke; queues up
        /// this event.
        /// </summary>
        public event EventHandler<IGameEvent> EventTriggered;

        /// <summary>
        /// Fires when the event completes.
        /// </summary>
        public event EventHandler<IGameEvent> EventFinished;

        /// <summary>
        /// Runs through the event
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator DoEvent();

        [Tooltip("The next event that should happen (or null if this is the last one)")]
        [SerializeField]
        private GameEventBase _followupEvent;
        public IGameEvent FollowupEvent => _followupEvent;

        /// <summary>
        /// Whether the event has ran to completion or has been cancelled.
        /// </summary>
        public bool IsCompleted { get; protected set; } = false;

        /// <summary>
        /// Whether or not this dialog event is possible
        /// (for example, if a needed character is alive).
        /// </summary>
        public abstract bool EventPossible();

        /// <summary>
        /// Initializes the event, such as by looking for relevant objects
        /// in map.
        /// </summary>
        public abstract void PreStartEvent();

        [ContextMenu("Trigger Event")]
        public void TriggerEvent()
        {
            EventTriggered?.Invoke(this, this);
        }

        protected void InvokeEventFinished()
        {
            IsCompleted = true;
            EventFinished?.Invoke(this, this);
        }

        public virtual void CancelEvent()
        {
            IsCompleted = true;
            if (FollowupEvent != null && !FollowupEvent.IsCompleted)
            {
                // recursively cancels events for followups
                FollowupEvent.CancelEvent();
            }
        }
    }
}
