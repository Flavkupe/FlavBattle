using UnityEngine;

namespace FlavBattle.Entities.Modifiers
{
    public enum CombatEffectModifierDuration
    {
        Turns,
        AllCombat,
    }

    /// <summary>
    /// A temporary buff that happens in combat. Has a fixed
    /// duration and generally ends at the end of combat.
    /// </summary>
    public class CombatEffectModifier : ModifierBase
    {
        private int _durationLeft = 0;
        private string _name;
        private UnitStats _buff;
        private CombatEffectModifierDuration _durationType;

        public CombatEffectModifier(string name, 
            UnitStats buff,
            CombatEffectModifierDuration durationType,
            int duration = 1)
        {
            _durationLeft = duration;
            _durationType = durationType;
            _name = name;
            _buff = buff;
        }

        public override ModifierType Type => ModifierType.Effect;

        public override bool IsExpired => _durationLeft < 0;

        // TODO
        public override bool AllowDuplicate => true;

        public override string Name => _name ?? "unknown";

        public override void Tick(ModifierTickType type)
        {
            if (type == ModifierTickType.CombatTurnStart &&
                _durationType == CombatEffectModifierDuration.Turns)
            {
                _durationLeft--;
            }

            if (type == ModifierTickType.CombatEnd)
            {
                _durationLeft = 0;
            }
        }

        public override void Apply(UnitStatSummary summary, Unit unit)
        {
            if (_buff == null)
            {
                Debug.LogWarning("No buff assigned for CombatEffectModifier");
                return;
            }

            summary.Tally(_buff, _name);
        }
    }
}
