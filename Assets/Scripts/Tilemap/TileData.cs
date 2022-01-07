﻿using System;
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
        [SerializeField]
        private string _name;
        public string Name => _name;

        public CombatFormationSlot SlotModel => _slotModel;

        [SerializeField]
        private CombatFormationSlot _slotModel;

        /// <summary>
        /// Whether the tile is passable by default. Use GridTile's GetPassableState
        /// to get the passable state while accounting for army modifiers
        /// and such.
        /// </summary>
        public bool Passable;

        /// <summary>
        /// Default walk cost of tile. Use GridTile's GetTileCost
        /// to get the cost while accounting for army modifiers
        /// and such. Cost of 1.0f is the default cost.
        /// </summary>
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

        [Tooltip("Stats that units get when they are on this tile.")]
        public UnitStats Stats;
    }
}
