using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyIdleState : ArmyMapStateBase
    {
        public override ArmyMapState State => ArmyMapState.Idle;

        public override ArmyStatePriority Priority => ArmyStatePriority.Last;

        public override bool ShouldTransitionToState(Army army)
        {
            // Idle state is when army is doing nothing
            return !army.HasDestination && !army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            // Does nothing
        }

        public override void EnterState(Army army)
        {
        }

        public override void ExitState(Army army)
        {
        }
    }
}
