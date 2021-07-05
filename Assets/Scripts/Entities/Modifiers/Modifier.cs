using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Modifiers
{
    public enum ModifierType
    {
        Default,
        Perk,
        Effect
    }

    public enum ModifierTickType
    {
        CombatStart,
        CombatTurnStart,
        CombatEnd,

        // TODO
        MapTileReached,
    }

    public interface IModifier
    {
        string Name { get; }
        ModifierType Type { get; }
        void Apply(UnitStatSummary summary, Unit unit);
        bool IsExpired { get; }
        bool AllowDuplicate { get; }
        void Tick(ModifierTickType type);


    }

    public abstract class ModifierBase : IModifier
    {
        public abstract ModifierType Type { get; }
        public abstract bool IsExpired { get; }
        public abstract bool AllowDuplicate { get; }
        public abstract string Name {get; }

        public abstract void Apply(UnitStatSummary summary, Unit unit);

        public virtual void Tick(ModifierTickType type)
        {
            // overridable
        }
    }
}
