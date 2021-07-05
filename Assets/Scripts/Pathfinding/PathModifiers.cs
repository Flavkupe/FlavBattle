using FlavBattle.Entities.Data;
using FlavBattle.Tilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Pathfinding
{
    public class PathModifier
    {
        /// <summary>
        /// How much to multiply the tile cost by.
        /// </summary>
        public float WalkCostMultiplier = 1.0f;

        /// <summary>
        /// Makes walkable tiles unwalkable and vice versa
        /// </summary>
        public bool ReverseWalkableState = false;


        public void Combine(PathModifier other)
        {
            ReverseWalkableState = ReverseWalkableState || other.ReverseWalkableState;

            // TODO: Walk cost takes the lowest cost from the group
            // Should it use a different approach?
            WalkCostMultiplier = Math.Max(0.1f, Math.Min(WalkCostMultiplier, other.WalkCostMultiplier));
        }
    }

    public class PathModifiers
    {
        Dictionary<BiomeType, PathModifier> _modifiers = new Dictionary<BiomeType, PathModifier>();

        public PathModifiers()
        {
        }

        /// <summary>
        /// Collects all path modifiers from the IArmy object and initializes
        /// a PathModifiers object with just those modifiers.
        /// </summary>
        /// <param name="army"></param>
        private PathModifiers(IArmy army)
        {
            this.FromArmy(army);
        }

        public static PathModifiers CreateFromArmy(IArmy army)
        {
            return new PathModifiers(army);
        }

        public void SetModifier(BiomeType type, PathModifier modifier)
        {
            _modifiers[type] = modifier;
        }

        public void AddModifier(BiomeType type, PathModifier modifier)
        {
            if (!_modifiers.ContainsKey(type))
            {
                SetModifier(type, modifier);
                return;
            }

            _modifiers[type].Combine(modifier);
        }

        public void AddCostModifier(BiomeType type, float speed)
        {
            AddModifier(type, new PathModifier() { WalkCostMultiplier = speed });
        }

        public void AddReversePassableModifier(BiomeType type, bool reverse)
        {
            AddModifier(type, new PathModifier() { ReverseWalkableState = reverse });
        }

        public PathModifier GetModifiers(params BiomeType[] types)
        {
            var combined = new PathModifier();
            foreach (var type in types)
            {
                var modifier = GetModifier(type);
                if (modifier != null)
                {
                    combined.Combine(modifier);
                }
            }

            return combined;
        }

        public PathModifier GetModifier(BiomeType type)
        {
            var modifier = _modifiers.GetValueOrDefault(type);
            var anyModifier = _modifiers.GetValueOrDefault(BiomeType.Any);
            if (anyModifier != null && modifier != null)
            {
                modifier.Combine(anyModifier);
            }

            return modifier ?? anyModifier;
        }

        public void Clear()
        {
            _modifiers.Clear();
        }

        public void FromArmy(IArmy army)
        {
            this.Clear();
            foreach (var unit in army.GetUnits())
            {
                foreach (var perk in unit.Info.Perks)
                {
                    foreach (var bonus in perk.Effects)
                    {
                        if (bonus.Effect != PerkBonus.Type.MapBonus)
                        {
                            continue;
                        }

                        if (bonus.MapBonus == PerkBonus.MapBonusType.MoveBonus)
                        {
                            AddCostModifier(bonus.Biome, bonus.Amount);
                        }

                        if (bonus.MapBonus == PerkBonus.MapBonusType.ReversePassable)
                        {
                            AddReversePassableModifier(bonus.Biome, bonus.IsSet);
                        }
                    }
                }
            }
        }
    }
}
