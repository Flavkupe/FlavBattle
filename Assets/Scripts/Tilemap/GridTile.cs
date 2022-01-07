using FlavBattle.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Tilemap
{
    public class GridTile : IEquatable<GridTile>
    {
        public IEnumerable<TileInfo> Props;
        public TileInfo MainTile;

        public int GridX;
        public int GridY;
        public float WorldX;
        public float WorldY;

        public Vector3 ToWorldPos(float z = 0.0f)
        {
            return new Vector3(WorldX, WorldY, 0.0f);
        }

        public Vector2 ToWorldPos2D()
        {
            return new Vector2(WorldX, WorldY);
        }

        public Vector2 ToGridPos()
        {
            return new Vector2(GridX, GridY);
        }

        public override string ToString()
        {
            return $"({GridX},{GridY})";
        }

        public bool Equals(GridTile other)
        {
            return this.GridX == other.GridX && this.GridY == other.GridY;
        }

        /// <summary>
        /// Returns true if this grid tile or any of its props match the biome type.
        /// </summary>
        public bool ContainsBiome(BiomeType type)
        {
            if (type == BiomeType.Any || this.MainTile?.BiomeType == type || this.Props.Any(a => a.BiomeType == type))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the modified tile cost of passing the tile after applying PathModifiers
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="pathModifiers"></param>
        /// <returns></returns>
        public float GetTileCost(PathModifiers pathModifiers)
        {
            var main = this.MainTile;
            var cost = main.WalkCost;
            foreach (var prop in this.Props)
            {
                cost += prop.WalkCost;
                if (main.OverrideWalk && !prop.OverrideWalk)
                {
                    cost = main.WalkCost;
                }
                else if (!main.OverrideWalk && prop.OverrideWalk)
                {
                    cost = prop.WalkCost;
                }
            }

            cost = Math.Max(0.1f, cost);

            var modifiers = GetCombinedTileModifier(pathModifiers);
            if (modifiers != null && modifiers.WalkCostMultiplier > 0.0f)
            {
                cost *= modifiers.WalkCostMultiplier;
            }

            return cost;
        }

        public bool GetPassableState(PathModifiers pathModifiers)
        {
            var main = this.MainTile;
            var passable = main.Passable || main.OverridePassable;

            foreach (var prop in this.Props)
            {
                if (!prop.Passable)
                {
                    passable = false;
                }

                if (prop.OverridePassable)
                {
                    // if any prop has OverridePassable, it's guaranteed passable
                    passable = true;
                    break;
                }
            }

            var modifiers = GetCombinedTileModifier(pathModifiers);
            if (modifiers != null && modifiers.ReverseWalkableState)
            {
                // invert passable status
                passable = !passable;
            }

            return passable;
        }

        /// <summary>
        /// Gets the path modifier for the tile and subtiles (rivers, etc)..
        /// 
        /// Returns null if there are no modifiers.
        /// </summary>
        private PathModifier GetCombinedTileModifier(PathModifiers pathModifiers)
        {
            var modifiers = pathModifiers.GetModifiers(this.MainTile.BiomeType);
            if (this.Props == null || this.Props.Count() == 0)
            {
                return modifiers;
            }


            var propModifiers = pathModifiers.GetModifiers(this.Props.Select(a => a.BiomeType).ToArray());
            if (propModifiers == null)
            {
                return modifiers;
            }

            modifiers.Combine(propModifiers);
            return modifiers;
        }
    }
}
