using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Map.State
{
    /// <summary>
    /// State for Computer-controlled armies that do their own stuff
    /// when not moving or fleeing
    /// </summary>
    public class ArmyAIDecisionState : ArmyMapStateBase
    {
        private List<ArmyMapAIBase> _aiActions = new List<ArmyMapAIBase>();
        private ArmyMapAIBase _currentAction = null;
        private ThrottleTimer _plotActionThrottle = new ThrottleTimer(1.0f);

        public override ArmyMapState State => ArmyMapState.AI;

        void Start()
        {
            _aiActions.AddRange(GetComponents<ArmyMapAIBase>());
            _aiActions = _aiActions.OrderBy(a => a.Priority).ToList();
        }

        public override bool ShouldTransitionToState(Army army)
        {
            // AI state is for the AI to decide on what to do next from custom (AI) actions
            return _aiActions.Count > 0 && !army.HasDestination && !army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            if (_plotActionThrottle.Tick(TimeUtils.FullAdjustedGameDelta))
            {
                PlotNextAction(army);
            }
        }

        public override void EnterState(Army army)
        {
        }

        public override void ExitState(Army army)
        {
        }

        private void PlotNextAction(Army army)
        {
            if (_aiActions.Count == 0)
            {
                return;
            }

            foreach (var action in _aiActions)
            {
                if (action.IsActionPossible(army, Tilemap))
                {
                    _currentAction = action;
                    action.DoAction(army, Tilemap);
                    return;
                }
            }
        }

    }
}
