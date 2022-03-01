using FlavBattle.Trace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.State
{
    public enum MapEventType
    {
        MapPaused,
        MapUnpaused,
    }

    public enum GameEventQueueType
    {
        Main,
        Combat,
    }

    public class CombatEndedEventArgs
    {
        public IArmy Winner { get; set; }
        public IArmy Loser { get; set; }

        public VictoryType VictoryType;
    }

    public class CombatStartedEventArgs
    {
        public IArmy Player { get; set; }
        public IArmy Enemy { get; set; }
    }

    public enum VictoryType
    {
        Destroyed,
        Fled,
        Routed,
    }

    /// <summary>
    /// Helper for state shortcuts, such as checking if
    /// game is paused
    /// </summary>
    public static class GameState
    {
        public static bool IsMapPaused => GameEventManager.IsMapPaused;
    }

    public class GameEventManager : MonoBehaviour, IHasTraceData
    {
        public event EventHandler<MapEventType> MapEvent;

        public event EventHandler<CombatEndedEventArgs> CombatEndedEvent;

        public event EventHandler<CombatStartedEventArgs> CombatStartedEvent;

        public event EventHandler<Unit> UnitDeployed;

        public event EventHandler<Unit> UnitGarrisoned;

        public event EventHandler AllGameEventsDone;
        public event EventHandler AllCombatEventsDone;

        [SerializeField]
        private KeyCode _cancelKey = KeyCode.Escape;

        /// <summary>
        /// Whether or not the map is paused (for example, Armies should not move)
        /// </summary>
        public static bool IsMapPaused => _mapPauseStack > 0;

        /// <summary>
        /// Counts how many times map was paused (in case multiple things pause and then unpause).
        /// </summary>
        private static int _mapPauseStack = 0;

        private GameEventQueue _queue;
        private GameEventQueue _combatQueue;

        // Start is called before the first frame update
        void Awake()
        {
            _queue = MiscUtils.MakeOfType<GameEventQueue>("EventQueue", this.transform);
            _queue.AllDone += HandleAllEventsDone;
            _queue.SetCancelKey(_cancelKey);

            _combatQueue = MiscUtils.MakeOfType<GameEventQueue>("CombatEventQueue", this.transform);
            _combatQueue.AllDone += HandleCombatEventsDone;
            _combatQueue.SetCancelKey(_cancelKey);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TimeUtils.GameSpeed.SetGameSpeed(GameSpeed.Slow);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TimeUtils.GameSpeed.SetGameSpeed(GameSpeed.Normal);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TimeUtils.GameSpeed.SetGameSpeed(GameSpeed.VeryFast);
            }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.BackQuote))
            {
                TimeUtils.GameSpeed.TogglePause();
            }
        }

        public void AddOrStartGameEvent(IGameEvent e, GameEventQueueType queue = GameEventQueueType.Main)
        {
            if (_queue.IsEmpty && queue == GameEventQueueType.Main)
            {
                // TODO: event-dependent
                TriggerMapEvent(MapEventType.MapPaused);
            }

            var specificQueue = queue == GameEventQueueType.Main ? _queue : _combatQueue;
            specificQueue.AddOrStartEvent(e);
        }

        /// <summary>
        /// Quickly triggers an event in the main loop. Can be called from
        /// Unity inspector.
        /// </summary>
        public void TriggerEvent(GameEventBase gameEvent)
        {
            AddOrStartGameEvent(gameEvent);
        }

        public void TriggerMapEvent(MapEventType mapEvent)
        {
            if (mapEvent == MapEventType.MapPaused)
            {
                Logger.Log(LogType.GameEvents, "Map paused");
                _mapPauseStack++;
            }
            else if (mapEvent == MapEventType.MapUnpaused)
            {
                Logger.Log(LogType.GameEvents, "Map unpaused");
                _mapPauseStack--;
            }

            Debug.Assert(_mapPauseStack >= 0, "Should never unpause more times than paused!");
            _mapPauseStack = Math.Max(0, _mapPauseStack); // floor it at 0, just in case

            MapEvent?.Invoke(this, mapEvent);
        }

        public void TriggerUnitDeployed(Unit unit)
        {
            UnitDeployed?.Invoke(this, unit);
        }

        public void TriggerCombatStartedEvent(IArmy player, IArmy enemy)
        {
            CombatStartedEvent?.Invoke(this, new CombatStartedEventArgs
            {
                Player = player,
                Enemy = enemy
            });
        }

        public void TriggerCombatEndedEvent(IArmy winner, IArmy loser, VictoryType type = VictoryType.Destroyed)
        {
            CombatEndedEvent?.Invoke(this, new CombatEndedEventArgs
            {
                Winner = winner,
                Loser = loser,
                VictoryType = type,
            });
        }
        public void TriggerUnitGarrisoned(Unit e)
        {
            UnitGarrisoned?.Invoke(this, e);
        }

        private void HandleAllEventsDone(object sender, EventArgs e)
        {
            // TODO: event-dependent
            TriggerMapEvent(MapEventType.MapUnpaused);
            AllGameEventsDone?.Invoke(this, e);
        }

        private void HandleCombatEventsDone(object sender, EventArgs e)
        {
            AllCombatEventsDone?.Invoke(this, e);
        }

        public TraceData GetTrace()
        {
            var trace = _queue?.GetTrace();
            return TraceData.TopLevelTrace("GameEventManager", trace);
        }
    }
}