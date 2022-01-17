using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Modifiers
{
    public class ArmyModifierSet : ModifierSet
    {
        public override string ID => "ArmyModifierSet";

        public IEnumerable<IArmyModifier> GetArmyModifiers()
        {
            return this.GetModifiers<IArmyModifier>();
        }

        public void UpdateModifiers(ICombatArmy army)
        {
            foreach (var modifier in GetArmyModifiers())
            {
                modifier.UpdateModifier(army);
            }
        }
    }

    public class UnitModifierSet : ModifierSet
    {
        public override string ID => "UnitModifierSet";
    }

    public abstract class ModifierSet : IModifier
    {
        private List<IModifier> _modifiers = new List<IModifier>();

        public abstract string ID { get; }

        public ModifierType Type => ModifierType.Group;

        public bool IsExpired => false;

        public bool AllowDuplicate => false;

        /// <summary>
        /// Gets all modifiers, flattening nested ModifierSets recursively.
        /// </summary>
        public IEnumerable<IModifier> GetModifiers()
        {
            var modifiers = new List<IModifier>();
            foreach (var modifier in _modifiers)
            {
                if (modifier is ModifierSet)
                {
                    modifiers.AddRange((modifier as ModifierSet).GetModifiers());
                }
                else
                {
                    modifiers.Add(modifier);
                }
            }

            return _modifiers;
        }

        protected IEnumerable<T> GetModifiers<T>() where T : IModifier
        {
            return GetModifiers().OfType<T>();
        }

        /// <summary>
        /// Applies each modifier to the summary, using info about the unit if
        /// applicable for the modifier.
        /// </summary>
        public void Apply(UnitStatSummary summary, Unit unit = null)
        {
            foreach (var modifier in _modifiers)
            {
                modifier.Apply(summary, unit);
            }
        }

        public void Tick(ModifierTickType type)
        {
            var keptModifiers = new List<IModifier>();
            foreach (var modifier in _modifiers)
            {
                modifier.Tick(type);
                if (!modifier.IsExpired)
                {
                    keptModifiers.Add(modifier);
                }
            }

            _modifiers = keptModifiers;
        }

        public void AddModifier(IModifier newModifier)
        {
            foreach (var modifier in _modifiers)
            {
                if (modifier.ID == newModifier.ID && !modifier.AllowDuplicate)
                {
                    // ignore dupes
                    return;
                }
            }

            _modifiers.Add(newModifier);
        }
    }
}
