using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Entities
{
    /// <summary>
    /// Summary of unit's attack and defense stats.
    /// Essentially, a tally of what causes attack and defense
    /// numbers for this unit.
    /// </summary>
    public class UnitStatSummary
    {
        public List<SummaryItem> Items { get; } = new List<SummaryItem>();

        private Dictionary<UnitStatType, int> _totals = new Dictionary<UnitStatType, int>();

        public class SummaryItem
        {
            public SummaryItem(UnitStatType type, int amount, string description)
            {
                Type = type;
                Amount = amount;
                Description = description;
            }

            /// <summary>
            /// What kind of item this is a summary of
            /// </summary>
            public UnitStatType Type;
            public int Amount;
            public string Description;
        }

        public UnitStatSummary()
        {
            foreach (var stat in UnitStats.Types)
            {
                _totals[stat] = 0;
            }
        }

        public void Clear()
        {
            Items.Clear();
            _totals.Clear();
        }

        /// <summary>
        /// Adds a stat and amount to the tally for the summary.
        /// Amounts of 0 are ignored.
        /// </summary>
        public void Tally(UnitStatType type, int amount, string description)
        {
            if (amount == 0)
            {
                return;
            }

            _totals.SetOrAddTo(type, amount);
            Items.Add(new SummaryItem(type, amount, description));
        }

        /// <summary>
        /// Goes through each stat and tallies it with the provided description.
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="description"></param>
        public void Tally(UnitStats stats, string description)
        {
            foreach (var stat in UnitStats.Types)
            {
                var value = stats.GetStatValue(stat);
                if (value != 0)
                {
                    Tally(stat, value, description);
                }
            }
        }

        public int GetTotal(UnitStatType type)
        {
            return _totals.GetValueOrDefault(type);
        }

        public UnitStats GetAccumulatedStats()
        {
            var stats = new UnitStats();
            stats.Power = _totals[UnitStatType.Power];
            stats.Defense = _totals[UnitStatType.Defense];
            stats.Speed = _totals[UnitStatType.Speed];
            stats.HP = _totals[UnitStatType.HP];
            stats.ActiveMoraleShields = _totals[UnitStatType.ActiveMoraleShields];
            stats.ActiveBlockShields = _totals[UnitStatType.ActiveBlockShields];
            stats.Commands = _totals[UnitStatType.Command];
            stats.StartingBlockShields = _totals[UnitStatType.StartingBlockShields];
            return stats;
        }

        /// <summary>
        /// Adds the info from the other UnitStatSummary,
        /// mutating this one.
        /// </summary>
        /// <param name="other"></param>
        public void Apply(UnitStatSummary other)
        {
            foreach (var key in this._totals.Keys.ToList())
            {
                _totals[key] += other._totals[key];
            }

            Items.AddRange(other.Items);
        }
    }
}
