using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Tilemap
{
    public enum SpecialTileProperty
    {
        None = 0,
        Garrison = 1,
        Town = 2,
    }

    [CreateAssetMenu(fileName = "Tile Data", menuName = "Custom/Tiles/Tile Data", order = 1)]
    public class TileData : ScriptableObject
    {
        public TileInfo Info;
    }

    [Serializable]
    public class TileInfo
    {
        public CombatFormationSlot SlotModel => _slotModel;

        [SerializeField]
        private CombatFormationSlot _slotModel;

        public bool Passable;
        public float WalkCost = 1.0f;

        public SpecialTileProperty SpecialProperty;

        [SerializeField]
        private BiomeType _biomeType;
        public BiomeType BiomeType => _biomeType;

        /// <summary>
        /// If true, the walk cost of this is used for computations
        /// rather than other stuff on the same tile.
        /// </summary>
        [Tooltip("Overrides walk cost of any other tiles.")]
        public bool OverrideWalk = false;

        /// <summary>
        /// If true, then the tile is passable no matter what else
        /// is here. Bridges are a good example.
        /// </summary>
        [Tooltip("If true, tile is passable no matter what else is here (example: bridge)")]
        public bool OverridePassable = false;

        public TileInfo Combine(TileInfo data)
        {
            var walkCost = WalkCost + data.WalkCost;
            if (OverrideWalk && !data.OverrideWalk)
            {
                walkCost = WalkCost;
            }
            else if (!OverrideWalk && data.OverrideWalk)
            {
                walkCost = data.WalkCost;
            }

            return new TileInfo()
            {
                Passable = OverridePassable || data.OverridePassable || (Passable && data.Passable),
                WalkCost = walkCost,

                // Technically this will randomly pick the first slot model to be non-null,
                // depending on order
                _slotModel = _slotModel ?? data._slotModel,
            };
        }
    }
}