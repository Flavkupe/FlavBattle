using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        IGameEvent FollowupEvent { get; }
        IEnumerator DoEvent();
        event EventHandler<IGameEvent> EventTriggered;
        event EventHandler<IGameEvent> EventFinished;
    }
}
