using System.Linq;
using System.Collections;
using UnityEngine;
using FlavBattle.State;

namespace FlavBattle.Combat.States
{
    /// <summary>
    /// Runs combat events like story dialog, and waits for them to complete.
    /// </summary>
    public class CombatEventsState : BattleStateBase
    {
        private bool _runningEvents = false;

        public CombatEventsState(MonoBehaviour owner) : base(owner)
        {
            
        }

        private void HandleAllGameEventsDone(object sender, System.EventArgs e)
        {
            _runningEvents = false;
        }

        public override bool ShouldUpdate(BattleStatus state)
        {
            return state.Stage == BattleStatus.BattleStage.CombatConditionalEvents;
        }

        protected override IEnumerator Run(BattleStatus state)
        {
            var gem = state.GameEventManager;
            gem.AllCombatEventsDone += HandleAllGameEventsDone;
            foreach (var conditionalEvent in state.ConditionalEvents.ToList())
            {
                if (conditionalEvent.MeetsConditions(state))
                {
                    _runningEvents = true;
                    state.ConditionalEvents.Remove(conditionalEvent);
                    gem.AddOrStartGameEvent(conditionalEvent.Event, GameEventQueueType.Combat);
                }
            }

            while (_runningEvents)
            {
                // wait for events to finish
                yield return null;
            }

            // no events; move on to next stage
            state.Stage = BattleStatus.BattleStage.SelectStance;
            gem.AllCombatEventsDone -= HandleAllGameEventsDone;
        }
    }
}

