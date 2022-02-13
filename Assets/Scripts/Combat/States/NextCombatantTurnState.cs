using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FlavBattle.Entities;

namespace FlavBattle.Combat.States
{
    public class NextCombatantTurnState : BattleStateBase
    {
        /// <summary>
        /// How long to stagger between parallel animations from
        /// same unit types.
        /// </summary>
        private float _parallelStaggerTime = 0.2f;

        public NextCombatantTurnState(MonoBehaviour owner) : base(owner)
        {
        }

        /// <summary>
        /// Executes if there are combatants in turn queue
        /// </summary>
        public override bool ShouldUpdate(BattleStatus state)
        {
            return state.Stage == BattleStatus.BattleStage.CombatPhase && (state.TurnQueue.Count > 0 || state.AbilityQueue.Count > 0);
        }

        protected override IEnumerator Run(BattleStatus state)
        {
            // AbilityQueue events get priority over TurnQueue
            if (state.AbilityQueue.Count > 0)
            {
                // Queued officer ability
                var officer = state.GetPlayerOfficer();
                var ability = state.AbilityQueue.Dequeue();
                yield return state.BattleUIPanel.AnimateAbilityNameCallout(ability);
                var summary = CombatUtils.ProcessTurn(state, officer, ability.Action);
                yield return DoTurn(state, summary);
            }
            else if (state.TurnQueue.Count > 0)
            {
                // Queued combatant actions
                var summary = CombatUtils.ProcessTurn(state);
                if (summary != null)
                {
                    yield return DoTurn(state, summary);
                }
            }
        }

        private IEnumerator DoTurn(BattleStatus state, CombatTurnSummary summary)
        {

            // Update panel as needed
            state.BattleUIPanel.AttackStats.SetStats(summary);

            var routineSet = new ParallelRoutineSet(_owner);
            routineSet.StaggerTime = _parallelStaggerTime;
            
            foreach (var turn in summary.Turns)
            {
                var animationGraph = turn.Ability.AnimationGraph;
                if (animationGraph == null)
                {
                    Debug.LogError($"Animation graph is null for ability '{turn.Ability.name}'");
                    continue;
                }

                var steps = animationGraph.GetStartStep(turn);

                routineSet.AddRoutine(steps.Do().ToRoutine());
            }

            // do actions in parallel
            yield return routineSet;

            // Step 5: check for newly dead units
            ProcessDeadUnits(state);

            AnimateUIForResults(state, summary);

            // Brief pause
            yield return new WaitForSecondsAccelerated(1.0f);
        }

        private void AnimateUIForResults(BattleStatus state, CombatTurnSummary summary)
        {
            if (summary.ArmyMoraleDamage > 0)
            {
                state.BattleUIPanel.AnimateMoraleBar(summary.FirstCombatant.IsInPlayerArmy, summary.ArmyMoraleDamage > 0);
            }

            state.BattleUIPanel.UpdateMorale(state.PlayerArmy, state.OtherArmy);
        }

        private void ProcessDeadUnits(BattleStatus state)
        {
            var deadUnits = CombatUtils.ProcessDeadUnits(state);

            foreach (var combatant in deadUnits)
            {
                combatant.CombatUnit.AnimateDeath();
                state.BattleUIPanel.AnimateMoraleBar(combatant.IsInPlayerArmy, false);
                state.BattleUIPanel.AnimateMoraleBar(!combatant.IsInPlayerArmy, true);
            }
        }
    }
}
