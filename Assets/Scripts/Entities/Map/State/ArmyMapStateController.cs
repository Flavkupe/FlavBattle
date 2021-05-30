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


            _army = GetComponent<Army>();

            InitStates(_army);            
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

        private void InitStates(Army army)
        {
            _allStates.Clear();
            _allStates.AddRange(GetComponentsInChildren<IArmyMapState>());
            _allStates = _allStates.OrderByDescending((a) => a.Priority).ToList();
        }

        private void CheckStates()
        {
            foreach (var state in _allStates)
            {
                if (state.ShouldSkip())
                {
                    // If checking a state with throttling, wait before
                    // checking that state again
                    continue;   
                }

                if (state.ShouldTransitionToState(_army))
                {
                    if (CurrentState != null)
                    {
                        if (state.Equals(CurrentState))
                        {
                            // Should stay at current state
                            break;
                        }

                        // Switching out of a different state
                        Debug.Log($"{_army.name} exiting state {CurrentState.State}");
                        CurrentState.SetActive(false);
                        CurrentState.ExitState(_army);
                    }


                    // Entering a new state
                    Debug.Log($"{_army.name} entering state {state.State}");
                    state.SetActive(true);
                    state.EnterState(_army);
                    CurrentState = state;
                    break;
                }
            }
        }
    }
}
