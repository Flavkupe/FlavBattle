using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities.Modifiers
{
    public class ModifierSet
    {
        public List<IModifier> Modifiers { get; private set; } = new List<IModifier>();

        public UnitStatSummary ApplyToStatSummary(UnitStatSummary summary, Unit unit)
        {
            foreach (var modifier in Modifiers)
            {
                modifier.Apply(summary, unit);
            }

            return summary;
        }

        public void TickModifiers(ModifierTickType type)
        {
            var keptModifiers = new List<IModifier>();
            foreach (var modifier in Modifiers)
            {
                modifier.Tick(type);
                if (!modifier.IsExpired)
                {
                    keptModifiers.Add(modifier);
                }
            }

            Modifiers = keptModifiers;
        }

        public void AddModifier(IModifier newModifier)
        {
            foreach (var modifier in Modifiers)
            {
                if (modifier.ID == newModifier.ID && !modifier.AllowDuplicate)
                {
                    // ignore dupes
                    return;
                }
            }

            Modifiers.Add(newModifier);
        }
    }
}
