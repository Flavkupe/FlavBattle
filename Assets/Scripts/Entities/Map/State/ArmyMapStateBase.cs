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
    }

    public abstract class ArmyMapStateBase : MonoBehaviour, IArmyMapState
    {
        protected TilemapManager Tilemap { get; private set; }

        public bool IsActive { get; private set; }

        public void Init(Army army, TilemapManager tilemapManager)
        {
            army.ArmyClicked += (obj, args) => OnArmyClicked(args);
            Tilemap = tilemapManager;
        }

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
    }
}
