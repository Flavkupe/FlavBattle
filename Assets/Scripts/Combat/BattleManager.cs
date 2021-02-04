using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using NaughtyAttributes;
using FlavBattle.Combat.States;
using FlavBattle.State;

namespace FlavBattle.Combat
{
    public class BattleManager : MonoBehaviour
    {
        private const float BattleFormationChangeCooldownSeconds = 5.0f;

        [Required]
        public BattleDisplay BattleDisplay;

        [Required]
        public BattleUIPanel BattleUIPanel;

        private BattleStatus _battleStatus;

        private List<IBattleState> _states = new List<IBattleState>();

        public BattleStatus GetBattleStatus()
        {
            return _battleStatus;
        }

        // Start is called before the first frame update
        void Start()
        {
            var gameEventManager = FindObjectOfType<GameEventManager>();
            _battleStatus = new BattleStatus(gameEventManager, BattleDisplay, BattleUIPanel);

            BattleUIPanel.OnStanceChangeClicked += (object o, FightingStance stance) => HandleStanceChanged(stance);
            BattleUIPanel.OnCommandAbilityUsed += HandleBattleUIPanelOnCommandAbilityUsed;
            BattleUIPanel.Hide();

            _states.Clear();

            // Add states in order. The order here matters! Top-most will always override if applicable.
            _states.AddRange(new List<IBattleState>()
        {
            // Must be first
            new InitCombatState(this),
            
            // Should happen soon after init
            new PreCombatBattleActionsState(this),

            // Most importantly should happen before NextCombatantTurnState
            new ArmyFleeingState(this),

            // Must be before NextCombatantTurnState
            new ShowWinnerState(this),

            // Should be towards the end, along with NextCombatantTurnState
            new InitRoundState(this),

            // Before new combat turn and after bout starts
            new StanceSelectionState(this, _battleStatus),

            // Must be after selecting stance, and before NextCombatantTurnState
            new DetermineTurnOrderState(this),

            // Should be towards the end, along with InitRoundState
            new NextCombatantTurnState(this),
        });


        }

        // Update is called once per frame
        void Update()
        {
            if (_battleStatus.Stage == BattleStatus.BattleStage.NotInCombat)
            {
                return;
            }

            foreach (var state in _states)
            {
                if (state.ShouldUpdate(_battleStatus))
                {
                    state.Update(_battleStatus);
                    // Only one state updates per cycle!
                    break;
                }
            }
        }

        public void StartCombat(IArmy player, IArmy enemy)
        {
            _battleStatus.Init(player, enemy);
            _battleStatus.Stage = BattleStatus.BattleStage.InitCombat;
        }

        /// <summary>
        /// Handles a UI stance change, such as from the UI or combat start
        /// </summary>
        /// <param name="stance"></param>
        private void HandleStanceChanged(FightingStance stance)
        {
            
        }

        /// <summary>
        /// Handles clicking on a command item from the CommandAbility panel.
        /// </summary>
        private void HandleBattleUIPanelOnCommandAbilityUsed(object sender, OfficerAbilityData e)
        {
            var officer = _battleStatus.GetPlayerOfficer();
            if (e.CommandCost <= officer.Unit.Info.CurrentStats.Commands)
            {
                _battleStatus.AbilityQueue.Enqueue(e);
                officer.Unit.Info.CurrentStats.Commands -= e.CommandCost;
                this.BattleUIPanel.CommandMenu.UpdateMenu();
            }
        }
    }
}