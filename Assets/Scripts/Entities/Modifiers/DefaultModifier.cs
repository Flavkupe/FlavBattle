using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Modifiers
{
    /// <summary>
    /// This modifier applies the basic modifiers such as stance and morale bonuses.
    /// It's present in all units unless explicitly replaced.
    /// </summary>
    public class DefaultModifier : ModifierBase
    {
        public override ModifierType Type => ModifierType.Default;

        /// <summary>
        /// Never expires
        /// </summary>
        public override bool IsExpired => false;

        /// <summary>
        /// Only one of this can exist
        /// </summary>
        public override bool AllowDuplicate => false;

        public override string Name => "Default Modifier";

        public override void Apply(UnitStatSummary summary, Unit unit, IArmy army)
        {
            ApplyAttackSummary(summary, unit, army);
            ApplyDefenseSummary(summary, unit, army);
        }

        private void ApplyAttackSummary(UnitStatSummary summary, Unit unit, IArmy army)
        {
            var type = UnitStatType.Power;

            var baseAttack = unit.Info.CurrentStats.Power;
            var moraleBonus = unit.Info.Morale.GetDefaultBonus();

            var attack = baseAttack;
            summary.Tally(type, baseAttack, "Base attack");

            attack += moraleBonus;
            summary.Tally(type, moraleBonus, "Morale");

            if (army.Stance == FightingStance.Offensive)
            {
                attack += 1;
                summary.Tally(type, 1, "Offensive stance");
            }
            else if (army.Stance == FightingStance.Defensive)
            {
                attack -= 1;
                summary.Tally(type, -1, "Defensive stance");
            }
        }


        /// <summary>
        /// Updates the stat summary of defense for the unit. This can
        /// later be acquired from the Unit's StatSummary property.
        /// </summary>
        private static void ApplyDefenseSummary(UnitStatSummary summary, Unit unit, IArmy army)
        {
            var type = UnitStatType.Defense;

            var baseDefense = unit.Info.CurrentStats.Defense;
            var moraleBonus = unit.Info.Morale.GetDefaultBonus();

            var defense = baseDefense;
            summary.Tally(type, baseDefense, "Base defense");

            defense += moraleBonus;
            summary.Tally(type, moraleBonus, "Morale");

            if (army.Stance == FightingStance.Offensive)
            {
                defense -= 1;
                summary.Tally(type, -1, "Offensive stance");
            }
            else if (army.Stance == FightingStance.Defensive)
            {
                defense += 1;
                summary.Tally(type, 1, "Defensive stance");
            }
        }
    }
}
