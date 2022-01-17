using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Modifiers
{
    public enum ModifierType
    {
        /// <summary>
        /// A generic modifier.
        /// </summary>
        Default,

        /// <summary>
        /// A perk for a unit which creates a modifier effect.
        /// </summary>
        Perk,

        /// <summary>
        /// A temporary or permanent effect like a buff or officer
        /// action effect.
        /// </summary>
        Effect,

        /// <summary>
        /// A modifier for an army on the map, such as a terrain bonus
        /// or flanking bonus.
        /// </summary>
        MapArmy,

        /// <summary>
        /// A grouping of modifiers, such as a ModifierSet
        /// </summary>
        Group,
    }

    public enum ModifierTickType
    {
        CombatStart,
        CombatTurnStart,
        CombatEnd,

        // TODO
        MapTileReached,
    }

    public interface IArmyModifier : IModifier
    {
        void UpdateModifier(ICombatArmy army);
    }

    public interface IModifier
    {
        /// <summary>
        /// Used to indentify different modifiers. Can be a descriptive name.
        /// </summary>
        string ID { get; }
        ModifierType Type { get; }

        /// <summary>
        /// Applies the modifier bonuses to the provided summary, possibly
        /// using stats from the passed-in unit. Unit can be null.
        /// </summary>
        void Apply(UnitStatSummary summary, Unit unit = null);
        bool IsExpired { get; }
        bool AllowDuplicate { get; }
        void Tick(ModifierTickType type);
    }

    public abstract class ModifierBase : IModifier
    {
        public abstract ModifierType Type { get; }
        public abstract bool IsExpired { get; }
        public abstract bool AllowDuplicate { get; }
        public abstract string ID {get; }

        public abstract void Apply(UnitStatSummary summary, Unit unit);

        public virtual void Tick(ModifierTickType type)
        {
            // overridable
        }
    }
}
