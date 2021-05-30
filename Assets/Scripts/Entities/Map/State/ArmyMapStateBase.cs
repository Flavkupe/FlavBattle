using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    public enum ArmyMapState
    {
        Unknown = 1,
        Idle = 2,
        MovingToNode = 3,
        Fleeing = 4,
        AI = 5,

        AIChargeToTarget = 6,
        AIPatrol = 7,
    }

    public enum ArmyStatePriority
    {
        // Idle state or the like
        Last = 0,

        // Things like basic movement, to be overridden by AI movement
        Default = 1,

        // Low prio AI choices - patrolling, etc
        LowAI = 2,

        // Primary AI choices - following, etc
        MidAI = 3,

        // Overrides all - fleeing, etc
        Highest = 100,
    }

    public interface IArmyMapState
    {
        ArmyMapState State { get; }
        void SetActive(bool active);
        bool ShouldTransitionToState(Army army);
        void DoUpdate(Army army);

        /// <summary>
        /// Happens when moving from a different state to this one.
        /// </summary>
        void EnterState(Army army);

        /// <summary>
        /// Happens when moving from a this state to a different one.
        /// </summary>
        void ExitState(Army army);

        void Init(Army army, TilemapManager tilemapManager);

        /// <summary>
        /// Whether the step should be explicitly skipped
        /// </summary>
        /// <returns></returns>
        bool ShouldSkip();

        ArmyStatePriority Priority { get; }
    }

    public abstract class ArmyMapStateBase : MonoBehaviour, IArmyMapState, IEquatable<ArmyMapStateBase>
    {
        protected TilemapManager Tilemap { get; private set; }

        public bool IsActive { get; private set; }

        private float _throttleAmount = 0.0f;

        public void Init(Army army, TilemapManager tilemapManager)
        {
            army.ArmyClicked += (obj, args) => OnArmyClicked(args);
            Tilemap = tilemapManager;
        }

        public abstract ArmyStatePriority Priority { get; }
        public abstract ArmyMapState State { get; }
        public void SetActive(bool active)
        {
            IsActive = active;
        }

        public abstract bool ShouldTransitionToState(Army army);

        public abstract void DoUpdate(Army army);

        public abstract void EnterState(Army army);

        public abstract void ExitState(Army army);        

        /// <summary>
        /// Override for when army has been clicked
        /// </summary>
        /// <param name="army"></param>
        protected virtual void OnArmyClicked(ArmyClickedEventArgs args)
        {
        }

        public bool Equals(ArmyMapStateBase other)
        {
            if (other == null)
            {
                return false;
            }

            return this.State == other.State;
        }

        /// <summary>
        /// Skips the current step for specified amount of time.
        /// If time is left null, uses throttle time for the duration.
        /// </summary>
        /// <param name="time"></param>
        protected void Skip(float time = 1.0f)
        {
            _throttleAmount = time;
        }

        /// <summary>
        /// If true, ignores the state for the ShouldTransitionToState check for some time.
        /// This throttle is reset when Skip is called. This should be used
        /// to avoid constantly retrying expensive operations like pathfinding
        /// when it fails.
        /// </summary>
        public virtual bool ShouldSkip()
        {
            if (_throttleAmount > 0.0f)
            {
                _throttleAmount -= TimeUtils.AdjustedGameDelta;
                return true;
            }

            return false;
        }
    }
}
