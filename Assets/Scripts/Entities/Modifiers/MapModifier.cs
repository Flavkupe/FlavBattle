using FlavBattle.Entities;
using FlavBattle.Entities.Modifiers;
using FlavBattle.Tilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Modifiers
{
    public abstract class MapModifier : ModifierBase, IArmyModifier
    {
        private string _name;
        public MapModifier(string name)
        {
            _name = name;
        }

        public override string ID => _name;

        public override ModifierType Type => ModifierType.MapArmy;

        public override bool IsExpired => false;

        public override bool AllowDuplicate => false;

        public abstract void UpdateModifier(ICombatArmy army);
    }

    /// <summary>
    /// Modifiers for position relative to other armies, such as flanks,
    /// nearby units, and linked units.
    /// </summary>
    public class RelativeArmyModifier : MapModifier
    {
        private int _numFlanks = 0;
        private int _numLinks = 0;

        public RelativeArmyModifier()
            :base("RelativeArmy")
        {
        }

        public override void Apply(UnitStatSummary summary, Unit unit)
        {
            var flankPenalty = _numFlanks - 1;
            if (flankPenalty > 0)
            {
                summary.Tally(UnitStatType.Defense, flankPenalty * -1, "Flanking Armies");
            }

            if (_numLinks > 0)
            {
                summary.Tally(UnitStatType.Power, _numLinks, "Linked Armies");
            }
        }

        public override void UpdateModifier(ICombatArmy army)
        {
            _numFlanks = army.GetFlankingArmies().Count();
            _numLinks = army.GetLinkedArmies().Count();
        }
    }

    /// <summary>
    /// Modifiers for position relative to other armies, such as flanks,
    /// nearby units, and linked units.
    /// </summary>
    public class ArmyMoraleModifier : MapModifier
    {
        private Morale _morale;
        

        public ArmyMoraleModifier()
            : base("ArmyMorale")
        {
        }

        public override void Apply(UnitStatSummary summary, Unit unit)
        {
            if (_morale == null)
            {
                return;
            }

            var bonus = _morale.GetDefaultBonus();
            if (bonus != 0)
            {
                summary.Tally(UnitStatType.Defense, bonus, "Army Morale");
                summary.Tally(UnitStatType.Power, bonus, "Army Morale");
            }
        }

        public override void UpdateModifier(ICombatArmy army)
        {
            _morale = army.Morale;
        }
    }

    /// <summary>
    /// Modifiers for position relative to other armies, such as flanks,
    /// nearby units, and linked units.
    /// </summary>
    public class MapTileModifier : MapModifier
    {
        private GridTile _tile;

        public MapTileModifier()
            : base("MapTile")
        {
        }

        public override void Apply(UnitStatSummary summary, Unit unit)
        {
            if (_tile == null)
            {
                return;
            }

            summary.Tally(_tile.MainTile.Stats, _tile.MainTile.Name);

            foreach (var info in _tile.Props)
            {
                summary.Tally(info.Stats, info.Name);
            }
        }

        public override void UpdateModifier(ICombatArmy army)
        {
            _tile = army.CurrentTileInfo;
        }
    }
}
