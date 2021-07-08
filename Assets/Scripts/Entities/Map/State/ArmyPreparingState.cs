using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyPreparingState : ArmyMapStateBase
    {
        private float _timeLeft = 1.0f;

        public override ArmyMapState State => ArmyMapState.Preparing;

        public override ArmyStatePriority Priority => ArmyStatePriority.Higher;

        public override bool ShouldTransitionToState(Army army)
        {
            // Idle state is when army is doing nothing
            return army.IsPreparing && !army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            _timeLeft -= TimeUtils.AdjustedGameDelta;
            if (_timeLeft <= 0.0f)
            {
                army.SetPreparing(false);
            }
        }

        public override void EnterState(Army army)
        {
            _timeLeft = army.PreparationTime;
        }

        public override void ExitState(Army army)
        {
        }
    }
}
