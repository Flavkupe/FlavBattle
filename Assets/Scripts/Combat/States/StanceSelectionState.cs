using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.States
{
    public class StanceSelectionState : BattleStateBase
    {
        private FightingStance? _stanceSelected;

        public StanceSelectionState(MonoBehaviour owner, BattleStatus state) : base(owner)
        {
            state.BattleUIPanel.OnStanceChangeClicked += HandleStanceChangeClicked;
        }

        /// <summary>
        /// Executes if there are combatants in turn queue
        /// </summary>
        public override bool ShouldUpdate(BattleStatus state)
        {
            return state.Stage == BattleStatus.BattleStage.SelectStance;
        }

        protected override IEnumerator Run(BattleStatus state)
        {
            yield return AwaitStanceSelection(state);
        }

        private IEnumerator AwaitStanceSelection(BattleStatus state)
        {
            _stanceSelected = null;
            state.BattleUIPanel.ShowStancePanel();
            while (!_stanceSelected.HasValue)
            {
                yield return null;
                if (_stanceSelected.HasValue)
                {
                    state.PlayerArmy.Stance = _stanceSelected.Value;
                    break;
                }
            }

            state.Stage = BattleStatus.BattleStage.CombatPhase;
        }

        private void HandleStanceChangeClicked(object source, FightingStance stance)
        {
            _stanceSelected = stance;
        }
    }
}