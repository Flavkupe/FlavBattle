using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyFleeingState : ArmyMapStateBase
    {
        public override ArmyStatePriority Priority => ArmyStatePriority.Highest;

        public enum ArmyFleeingOption
        {
            /// <summary>
            /// Fleeing path set by default game rules
            /// (such as furthest tile, etc)
            /// </summary>
            Default,

            /// <summary>
            /// Army flees to nearest exit point. If path is impossible,
            /// reverts to default.
            /// </summary>
            ExitPoint,
        }

        [Tooltip("What sort of flee behavior to exhibit")]
        [SerializeField]
        private ArmyFleeingOption _option = ArmyFleeingOption.Default;

        private bool ShowExitPointOptions() => _option == ArmyFleeingOption.ExitPoint;

        [ShowIf("ShowExitPointOptions")]
        [Tooltip("What sort of flee behavior to exhibit")]
        [SerializeField]
        private Transform[] _exitPoints;

        [ShowIf("ShowExitPointOptions")]
        [Tooltip("Whether to vanish once exit point is reached.")]
        [SerializeField]
        private bool _vanishOnExitPoint = true;

        public override ArmyMapState State => ArmyMapState.Fleeing;

        public override bool ShouldTransitionToState(Army army)
        {
            // State for when army is fleeing
            return army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            if (army.HasDestination)
            {
                army.StepTowardsDestination();
            }
            else
            {
                // Done fleeing
                army.SetFleeing(false);
                if (_option == ArmyFleeingOption.ExitPoint && _vanishOnExitPoint)
                {
                    army.FleeMap();
                }
            }
        }

        public override void EnterState(Army army)
        {
            if (_option == ArmyFleeingOption.Default)
            {
                return;
            }
            else if (_option == ArmyFleeingOption.ExitPoint)
            {
                SetPathToExitPoints(army);
            }
        }

        public override void ExitState(Army army)
        {
        }

        private void SetPathToExitPoints(Army army)
        {
            var exits = _exitPoints.Select(a => a.position);
            var path = Tilemap.GetFastestPathFromWorldPos(army.transform.position, exits);
            army.SetPath(path);
        }
    }
}
