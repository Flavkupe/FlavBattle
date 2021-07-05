using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyPatrollingState : ArmyMapStateBase
    {
        [SerializeField]
        private PatrolPoint[] _patrolPoints;

        [SerializeField]
        private float _patrolDelay = 1.0f;

        private int _currentPatrolIndex = 0;

        private ThrottleTimer _delayTimer = new ThrottleTimer(0.0f);

        public override ArmyStatePriority Priority => ArmyStatePriority.LowAI;
        public override ArmyMapState State => ArmyMapState.AIPatrol;

        public override bool ShouldTransitionToState(Army army)
        {
            if (_patrolPoints.Length == 0)
            {
                Debug.LogWarning("No patrol points!");
                return false;
            }

            return !army.IsFleeing;
        }

        public override void DoUpdate(Army army)
        {
            if (!army.HasDestination)
            {
                if (!_delayTimer.Tick())
                {
                    // throttle
                    return;
                }

                var patrolPoint = _patrolPoints[_currentPatrolIndex];
                var currentTile = Tilemap.GetGridTileAtWorldPos(army.gameObject);
                var targetTile = Tilemap.GetGridTileAtWorldPos(patrolPoint.gameObject);

                if (currentTile.Equals(targetTile) && _patrolPoints.Length > 1)
                {
                    // go to next patrol point
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
                    patrolPoint = _patrolPoints[_currentPatrolIndex];
                    targetTile = Tilemap.GetGridTileAtWorldPos(patrolPoint.gameObject);
                }

                var path = Tilemap.GetPath(currentTile, targetTile, army.GetPathModifiers());
                if (path == null)
                {
                    this.Skip(2.0f);
                    return;
                }

                army.SetPath(path);
                _delayTimer = new ThrottleTimer(_patrolDelay);
            }

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
