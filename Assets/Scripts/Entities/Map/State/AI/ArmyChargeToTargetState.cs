using System;
using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyChargeToTargetState : ArmyMapStateBase
    {
        public override ArmyStatePriority Priority => ArmyStatePriority.MidAI;

        public GameObject Target;

        public override ArmyMapState State => ArmyMapState.AIChargeToTarget;

        private void Start()
        {
            if (Target.GetComponent<IDetectable>() == null)
            {
                Debug.LogWarning($"Target for ${this.name} is not IDetectable");
            }
        }

        public override void DoUpdate(Army army)
        {
            army.StepTowardsDestination();
        }

        public override void EnterState(Army army)
        {
            if (Tilemap == null)
            {
                return;
            }

            var path = Tilemap.GetPath(army.gameObject, Target, army.GetPathModifiers());
            if (path == null)
            {
                // path not found; throttle this state for some time
                this.Skip(2.0f);
                return;
            } else
            {
                army.SetPath(path);
            }
        }

        public override void ExitState(Army army)
        {
        }

        public override bool ShouldTransitionToState(Army army)
        {
            if (HasReachedTarget(army))
            {
                // already reached target
                return false;
            }

            return true;
        }

        private bool HasReachedTarget(Army army)
        {
            var detectable = Target.GetComponent<IDetectable>();
            return army.Detects(detectable);
        }
    }
}
