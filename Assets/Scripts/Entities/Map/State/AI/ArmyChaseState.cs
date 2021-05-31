using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    public class ArmyChaseState : ArmyMapStateBase
    {
        public override ArmyStatePriority Priority => ArmyStatePriority.MidAI;

        public override ArmyMapState State => ArmyMapState.AIChase;

        private GameObject _target = null;

        [SerializeField]
        [Required]
        private Detector _chaseRange;

        public override void DoUpdate(Army army)
        {
            army.StepTowardsDestination();
        }

        public override void EnterState(Army army)
        {
            if (_target?.gameObject == null)
            {
                Debug.LogWarning("Entering state with no target!");
                this.Skip();
                return;
            }

            var path = Tilemap.GetPath(army.gameObject, _target.gameObject);
            army.SetPath(path);
        }

        public override void ExitState(Army army)
        {
        }

        public override bool ShouldTransitionToState(Army army)
        {
            return DetectsEnemy(army);   
        }

        private bool DetectsEnemy(Army army)
        {
            var detected = false;
            foreach (var other in _chaseRange.GetDetected<Army>(DetectableType.Army)) {
                if (other != null && other.Faction.Faction != army.Faction.Faction)
                {
                    detected = true;
                    _target = other.gameObject;
                    break;
                }
            }

            if (!detected)
            {
                _target = null;
                return false;
            }

            return true;
        }
    }
}