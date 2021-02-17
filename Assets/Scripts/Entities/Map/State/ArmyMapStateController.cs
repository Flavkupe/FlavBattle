using FlavBattle.State;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Map.State
{
    [RequireComponent(typeof(ArmyMovingToDestinationState))]
    [RequireComponent(typeof(ArmyFleeingState))]
    [RequireComponent(typeof(ArmyIdleState))]
    [RequireComponent(typeof(Army))]
    public class ArmyMapStateController : MonoBehaviour
    {
        public IArmyMapState CurrentState { get; private set; }

        [Tooltip("Ordered list of additional states to be in (if possible).")]
        [SerializeField]
        private ArmyMapStateBase[] _customStates;

        private List<IArmyMapState> _allStates = new List<IArmyMapState>();

        private Army _army;

        private TilemapManager _tilemapManager;

        void Start()
        {
            _tilemapManager = FindObjectOfType<TilemapManager>();
            if (_tilemapManager == null)
            {
                Debug.LogError("No _tilemapManager found for controller!");
            }

            InitStates();

            _army = GetComponent<Army>();
            foreach (var state in _allStates)
            {
                state.Init(_army, _tilemapManager);
            }

            CheckStates();
        }

        void Update()
        {
            if (GameState.IsMapPaused)
            {
                return;
            }

            if (CurrentState != null)
            {
                CurrentState.DoUpdate(_army);
            }

            // TODO: throttle?
            CheckStates();
        }

        private void InitStates()
        {
            _allStates.Clear();

            // If not available as custom states, add the "defaults".
            if (!_customStates.Any(a => a.State == ArmyMapState.MovingToNode))
            {
                _allStates.Add(GetComponent<ArmyMovingToDestinationState>());
            }
            if (!_customStates.Any(a => a.State == ArmyMapState.Fleeing))
            {
                _allStates.Add(GetComponent<ArmyFleeingState>());
            }

            _allStates.AddRange(_customStates);

            // Idle state should always be last one
            if (!_customStates.Any(a => a.State == ArmyMapState.Idle))
            {
                _allStates.Add(GetComponent<ArmyIdleState>());
            }
        }

        private void CheckStates()
        {
            foreach (var state in _allStates)
            {
                if (state.ShouldTransitionToState(_army))
                {
                    if (CurrentState != null)
                    {
                        if (state.State == CurrentState.State)
                        {
                            // Should stay at current state
                            break; ;
                        }

                        // Switching out of a different state
                        // Debug.Log($"{_army.name} exiting state {CurrentState.State}");
                        CurrentState.SetActive(false);
                        CurrentState.ExitState(_army);
                    }


                    // Entering a new state
                    // Debug.Log($"{_army.name} entering state {state.State}");
                    state.SetActive(true);
                    state.EnterState(_army);
                    CurrentState = state;
                    break;
                }
            }
        }
    }
}
