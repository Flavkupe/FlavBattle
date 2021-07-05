using FlavBattle.Tilemap;
using NaughtyAttributes;
using System;
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

            /// <summary>
            /// Inverts passable state for a passable/impassable
            /// biome.
            /// </summary>
            ReversePassable,
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

        private bool ShowAmount() => ShowMapffects() && MapBonus == MapBonusType.MoveBonus;

        private bool ShowBoolean() => ShowMapffects() && MapBonus == MapBonusType.ReversePassable;

        [SerializeField]
        private UnitStats _stats;
        public UnitStats Stats => _stats;

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
        [Tooltip("Threshold for condition")]
        private int _threshold;
        public int Threshold => _threshold;

        [ShowIf("ShowAmount")]
        [SerializeField]
        [ReorderableList]
        [AllowNesting]
        [Tooltip("Value for modifier")]
        private float _amount;
        public float Amount => _amount;

        [ShowIf("ShowBoolean")]
        [SerializeField]
        [ReorderableList]
        [AllowNesting]
        [Tooltip("True/False state for modifier")]
        private bool _isSet;
        public bool IsSet =>_isSet;
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
