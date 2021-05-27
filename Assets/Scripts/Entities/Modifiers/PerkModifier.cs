using FlavBattle.Entities.Data;

namespace FlavBattle.Entities.Modifiers
{
    public class PerkModifier : ModifierBase
    {
        private PerkData _data;

        public PerkModifier(PerkData data)
        {
            _data = data;
        }

        public override ModifierType Type => ModifierType.Perk;

        /// <summary>
        /// Perks never expire
        /// </summary>
        public override bool IsExpired => false;

        /// <summary>
        /// Perks can never be duplicated
        /// </summary>
        public override bool AllowDuplicate => false;

        public override string Name => _data.Name;

        public override void Apply(UnitStatSummary summary, Unit unit, IArmy army)
        {
            foreach (var effect in _data.Effects)
            {
                if (effect.Effect != PerkBonus.Type.StatBonus)
                {
                    continue;
                }

                if (UnitFullfillsPerkCondition(unit, army, effect))
                {
                    var perkName = $"{_data.Name} (Perk)";
                    summary.Tally(effect.Stats, perkName);
                }
            }
        }

        private bool UnitFullfillsPerkCondition(Unit unit, IArmy army, PerkBonus perk)
        {
            var condition = perk.BoostCondition;
            if (condition == PerkBonus.StatBoostConditionType.None)
            {
                return true;
            }

            if (unit != null)
            {
                if (condition == PerkBonus.StatBoostConditionType.MoraleOver && unit.Info.Morale.Current > perk.Threshold)
                {
                    return true;
                }
                if (condition == PerkBonus.StatBoostConditionType.MoraleUnder && unit.Info.Morale.Current < perk.Threshold)
                {
                    return true;
                }
            }

            if (army != null)
            {
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
            }

            return false;
        }
    }
}
