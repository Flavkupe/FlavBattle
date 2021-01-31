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
        public enum SummaryItemType
        {
            Attack,
            Defense,
        }

        public List<SummaryItem> Items { get; } = new List<SummaryItem>();

        private Dictionary<SummaryItemType, int> _totals = new Dictionary<SummaryItemType, int>();

        public class SummaryItem
        {
            public SummaryItem(SummaryItemType type, int amount, string description)
            {
                Type = type;
                Amount = amount;
                Description = description;
            }

            /// <summary>
            /// What kind of item this is a summary of
            /// </summary>
            public SummaryItemType Type;
            public int Amount;
            public string Description;
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
        public void Tally(SummaryItemType type, int amount, string description)
        {
            if (amount == 0)
            {
                return;
            }

            _totals.SetOrAddTo(type, amount);
            Items.Add(new SummaryItem(type, amount, description));
        }

        public int GetTotal(SummaryItemType type)
        {
            return _totals.GetValueOrDefault(type);
        }
    }
}
