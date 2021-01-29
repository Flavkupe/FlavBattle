using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Combat.States
{
    class DetermineTurnOrderState : BattleStateBase
    {
        public DetermineTurnOrderState(MonoBehaviour owner) : base(owner)
        {
        }

        /// <summary>
        /// Executes after stance has been picked and when ready to determine order of units
        /// </summary>
        public override bool ShouldUpdate(BattleStatus state)
        {
            return state.Stage == BattleStatus.BattleStage.DetermineTurnOrder && state.TurnQueue.Count == 0;
        }

        protected override IEnumerator Run(BattleStatus state)
        {
            var allCombatants = new HashSet<Combatant>(state.Combatants);

            // Get list of combatants with instant abilities first
            var instantAbilityCombatants = allCombatants.Where(a => CombatUtils.PickAction(state, a).InstantAbility);
            instantAbilityCombatants = instantAbilityCombatants.OrderByDescending(a => a.CombatCombinedStats.Speed);
            foreach (var combatant in instantAbilityCombatants)
            {
                allCombatants.Remove(combatant);
                state.TurnQueue.Enqueue(combatant);
            }

            // Queue up rest of combatants
            var otherCombatants = allCombatants.OrderByDescending(a => a.CombatCombinedStats.Speed);
            foreach (var combatant in otherCombatants)
            {
                state.TurnQueue.Enqueue(combatant);
            }

            state.Stage = BattleStatus.BattleStage.CombatPhase;
            yield break;
        }
    }
}
