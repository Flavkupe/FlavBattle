using FlavBattle.Tilemap;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Entities.Data
{
    [Serializable]
    public class PerkBonus
    {
        public enum Type
        {
            StatBonus,
            MapBonus,
        }

        public enum MapBonusType
        {
            MoveBonus,
            MakePassable,
        }

        public enum StatBoostConditionType
        {
            None,
            DefensiveStance,
            NeutralStance,
            OffensiveStance,
            MoraleOver,
            MoraleUnder,
        }

        [SerializeField]
        [AllowNesting]
        private Type _effect;
        public Type Effect => _effect;

        private bool ShowStatEffects() => Effect == Type.StatBonus;

        private bool ShowMapffects() => Effect == Type.MapBonus;

        private bool ShowThreshold() => BoostCondition == StatBoostConditionType.MoraleOver
            || BoostCondition == StatBoostConditionType.MoraleUnder;

        [SerializeField]
        private UnitStats _stats;
        public UnitStats Stats => _stats;

        [ShowIf("ShowStatEffects")]
        [SerializeField]
        [AllowNesting]
        private StatBoostConditionType _boostCondition;
        public StatBoostConditionType BoostCondition => _boostCondition;

        [ShowIf("ShowMapffects")]
        [SerializeField]
        [AllowNesting]
        private MapBonusType _mapBonus;
        public MapBonusType MapBonus => _mapBonus;

        [ShowIf("ShowMapffects")]
        [SerializeField]
        [ReorderableList]
        [AllowNesting]
        private BiomeType _biome;
        public BiomeType Biome => _biome;

        [ShowIf("ShowThreshold")]
        [SerializeField]
        [ReorderableList]
        [AllowNesting]
        private int _threshold;
        public int Threshold => _threshold;
    }

    [CreateAssetMenu(fileName = "Perk", menuName = "Custom/Perks/Perk Data", order = 1)]
    public class PerkData : ScriptableObject
    {
        [ShowAssetPreview(128, 128)]
        [AssetIcon]
        [SerializeField]
        private Sprite _icon;
        public Sprite Icon => _icon;

        [SerializeField]
        private string _name;
        public string Name => _name;

        [SerializeField]
        private string _description;
        public string Description => _description;

        [SerializeField]
        [ReorderableList]
        private PerkBonus[] _effects;
        public PerkBonus[] Effects => _effects;

        [SerializeField]
        private string _descriptionSuffix = "(Perk)";
        public string DescriptionSuffix => _descriptionSuffix;
    }
}
