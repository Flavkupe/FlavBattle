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
        /// <summary>
        /// A name of the event, for logging purposes.
        /// </summary>
        string EventName { get; }

        bool EventPossible();

        /// <summary>
        /// Whether the event will announce when it ends (true),
        /// or whether it will be run to completion (false). If this
        /// is true, event must call EventFinished when done.
        /// </summary>
        bool IsAsyncEvent { get; }

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
        /// Whether the event can be skipped
        /// </summary>
        bool IsSkippable { get; }

        /// <summary>
        /// Marks event to be cancelled. Sets Triggered to true.
        /// If a followup event cannot be skipped, will return that event.
        /// </summary>
        IGameEvent TrySkipEvent();
    }

    public abstract class GameEventBase : MonoBehaviour, IGameEvent
    {
        public string EventName => this.name;

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

        public abstract bool IsAsyncEvent { get; }

        /// <summary>
        /// Whether the event has ran to completion or has been cancelled.
        /// </summary>
        public bool IsCompleted { get; protected set; } = false;

        public virtual bool IsSkippable => true;

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

        /// <summary>
        /// Recursively attempts to skip the event and each
        /// followup event. If the event cannot be skipped,
        /// will run that event next.
        /// </summary>
        public virtual IGameEvent TrySkipEvent()
        {
            if (!IsSkippable)
            {
                // If the event cannot be skipped, return this event
                return this;
            }

            IsCompleted = true;
            if (FollowupEvent != null && !FollowupEvent.IsCompleted)
            {
                // recursively cancels events for followups
                var result = FollowupEvent.TrySkipEvent();
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
