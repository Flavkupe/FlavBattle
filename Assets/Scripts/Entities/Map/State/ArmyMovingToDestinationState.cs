using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyMovingToDestinationState : ArmyMapStateBase
    {
        public override ArmyStatePriority Priority => ArmyStatePriority.Default;
        public override ArmyMapState State => ArmyMapState.MovingToNode;

        public override bool ShouldTransitionToState(Army army)
        {
            // State for when army is moving normally, without fleeing
            return army.HasDestination && !army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            army.StepTowardsDestination();
        }

        public override void EnterState(Army army)
        {
        }

        public override void ExitState(Army army)
        {
        }
    }
}
