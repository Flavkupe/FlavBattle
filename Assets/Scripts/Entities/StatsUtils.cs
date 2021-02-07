using FlavBattle.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities
{
    public static class StatsUtils
    {
        public static UnitStats GetStatBonusesFromPerks(Unit unit, IArmy army)
        {
            var bonuses = new UnitStats();
            foreach (var perk in unit.Info.Perks)
            {
                foreach (var effect in perk.Effects)
                {
                    if (effect.Effect != PerkBonus.Type.StatBonus)
                    {
                        continue;
                    }

                    if (UnitFullfillsPerkCondition(unit, army, effect))
                    {
                        bonuses.Combine(effect.Stats);
                    }
                }
            }

            // Power and defense bonuses are set thru summary, not thru here.
            bonuses.Power = 0;
            bonuses.Defense = 0;
            return bonuses;
        }

        public static void ApplyPerkBonusesToSummary(Unit unit, IArmy army)
        {
            var summary = unit.StatSummary;
            foreach (var perk in unit.Info.Perks)
            {
                foreach (var effect in perk.Effects)
                {
                    if (effect.Effect != PerkBonus.Type.StatBonus)
                    {
                        continue;
                    }

                    if (UnitFullfillsPerkCondition(unit, army, effect))
                    {
                        var perkName = $"{perk.Name} (Perk)";
                        summary.Tally(UnitStatSummary.SummaryItemType.Attack, effect.Stats.Power, perkName);
                        summary.Tally(UnitStatSummary.SummaryItemType.Defense, effect.Stats.Defense, perkName);
                    }
                }
            }
        }

        public static bool UnitFullfillsPerkCondition(Unit unit, IArmy army, PerkBonus perk)
        {
            var condition = perk.BoostCondition;
            if (condition == PerkBonus.StatBoostConditionType.None)
            {
                return true;
            }
            if (condition == PerkBonus.StatBoostConditionType.MoraleOver && unit.Info.Morale.Current > perk.Threshold)
            {
                return true;
            }
            if (condition == PerkBonus.StatBoostConditionType.MoraleUnder && unit.Info.Morale.Current < perk.Threshold)
            {
                return true;
            }
            if (condition == PerkBonus.StatBoostConditionType.OffensiveStance && army.Stance == FightingStance.Offensive)
            {
                return true;
            }
            if (condition == PerkBonus.StatBoostConditionType.DefensiveStance && army.Stance == FightingStance.Defensive)
            {
                return true;
            }
            if (condition == PerkBonus.StatBoostConditionType.NeutralStance && army.Stance == FightingStance.Neutral)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the stat summary of attack for the unit. This can
        /// later be acquired from the Unit's StatSummary property.
        /// 
        /// Does not include boosts from actual abilities.
        /// </summary>
        public static void ComputeAttackSummary(Unit unit, IArmy army, UnitStats buffs = null)
        {
            var type = UnitStatSummary.SummaryItemType.Attack;
            var summary = unit.StatSummary;

            var baseAttack = unit.Info.CurrentStats.Power;
            var buffAttack = buffs?.Power ?? 0;
            var moraleBonus = unit.Info.Morale.GetDefaultBonus();

            var attack = baseAttack;
            summary.Tally(type, baseAttack, "Base attack");

            attack += buffAttack;
            summary.Tally(type, buffAttack, "Combat effects");

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
        public static void ComputeDefenseSummary(Unit unit, IArmy army, UnitStats buffs = null)
        {
            var type = UnitStatSummary.SummaryItemType.Defense;
            var summary = unit.StatSummary;

            var baseDefense = unit.Info.CurrentStats.Defense;
            var buffDefense = buffs?.Defense ?? 0;
            var moraleBonus = unit.Info.Morale.GetDefaultBonus();

            var defense = baseDefense;
            summary.Tally(type, baseDefense, "Base defense");

            defense += buffDefense;
            summary.Tally(type, buffDefense, "Combat effects");

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
