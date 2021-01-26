using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FlavBattle.Entities.Data
{
    public abstract class UnitData : ScriptableObject
    {
        [ShowAssetPreview(128, 128)]
        [AssetIcon]
        [SerializeField]
        private Sprite _icon;
        public virtual Sprite Icon => _icon;

        [SerializeField]
        private string _className;
        public virtual string ClassName => _className;

        [Tooltip("Debug option to always use lowest roll for stat rolls.")]
        [SerializeField]
        protected bool _rollLow = false;

        [BoxGroup("Base Stats")]
        [MinMaxSlider(0.0f, 30.0f)]
        [SerializeField]
        private Vector2 _hp;
        public Vector2 HP => _hp;

        [BoxGroup("Base Stats")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _power;
        public Vector2 Power => _power;

        [BoxGroup("Base Stats")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _defense;
        public Vector2 Defense => _defense;

        [BoxGroup("Base Stats")]
        [MinMaxSlider(0.0f, 10.0f)]
        [SerializeField]
        private Vector2 _speed;
        public Vector2 Speed => _speed;

        [BoxGroup("Stat Scaling")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _hpScaling;
        public Vector2 HPScaling => _hpScaling;

        [BoxGroup("Stat Scaling")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _powerScaling;
        public Vector2 PowerScaling => _powerScaling;

        [BoxGroup("Stat Scaling")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _defenseScaling;
        public Vector2 DefenseScaling => _defenseScaling;

        [BoxGroup("Stat Scaling")]
        [MinMaxSlider(0.0f, 5.0f)]
        [SerializeField]
        private Vector2 _speedScaling;
        public Vector2 SpeedScaling => _speedScaling;

        [Tooltip("How many bouts the unit will wait before wanting to flee combat when morale is low. 3 means they might flee on bout 3. Army calculates based on average.")]
        [SerializeField]
        private int _boutsToFlee = 3;
        public int BoutsToFlee => _boutsToFlee;

        [BoxGroup("Special Stats")]
        [SerializeField]
        private int _startingBlockShields;
        public int StartingBlockShields => _startingBlockShields;

        [BoxGroup("Visual")]
        [SerializeField]
        private Sprite _sprite;
        public Sprite Sprite => _sprite;

        [BoxGroup("Visual")]
        [SerializeField]
        [Required]
        private AnimatorOverrideController _animatorOverride;
        public virtual AnimatorOverrideController Animator => _animatorOverride;

        [BoxGroup("Visual")]
        [SerializeField]
        private Sprite[] _portraits;
        public Sprite[] Portraits => _portraits;

        [BoxGroup("Visual")]
        [ReorderableList]
        [SerializeField]
        private Sprite[] _animations;
        public virtual Sprite[] Animations => _animations;

        [BoxGroup("Abilities")]
        [SerializeField]
        private CombatAction[] _startingActions;
        public virtual CombatAction[] StartingActions => _startingActions;


        [BoxGroup("Abilities")]
        [Tooltip("Officer abilities that are always available by default")]
        [SerializeField]
        private OfficerAbilityData[] _defaultOfficerAbilities;
        public OfficerAbilityData[] DefaultOfficerAbilities => _defaultOfficerAbilities;

        [BoxGroup("Abilities")]
        [Tooltip("Officer abilities that can be learned")]
        [SerializeField]
        private OfficerAbilityData[] _officerAbilities;
        public OfficerAbilityData[] OfficerAbilities => _officerAbilities;

        /// <summary>
        /// Roll unit stats from the possible base props
        /// </summary>
        /// <returns></returns>
        public virtual UnitStats RollStartingStats(int level)
        {
            var stats = new UnitStats();
            stats.Level = level;
            stats.HP = GenerateStat(HP);
            stats.Power = GenerateStat(Power);
            stats.Defense = GenerateStat(Defense);
            stats.Speed = GenerateStat(Speed);
            stats.StartingBlockShields = StartingBlockShields;
            for (int i = 1; i < level; i++)
            {
                var levelup = RollLevel();
                stats = stats.Combine(levelup);
            }

            return stats;
        }

        public virtual UnitStats RollLevel()
        {
            var stats = new UnitStats();
            stats.HP = GenerateStat(HPScaling);
            stats.Power = GenerateStat(PowerScaling);
            stats.Defense = GenerateStat(DefenseScaling);
            stats.Speed = GenerateStat(SpeedScaling);
            return stats;
        }

        public virtual OfficerAbilityData RollNewOfficerAbility(int level, List<OfficerAbilityData> existing)
        {
            if (OfficerAbilities == null || OfficerAbilities.Length == 0)
            {
                return null;
            }

            // Find abilities that match min level and are not already known
            var available = OfficerAbilities.Where(a => a.MinLevel <= level && !existing.Any(b => b.Name == a.Name)).ToList();
            if (available.Count == 0)
            {
                // no viable abilities
                return null;
            }

            return available.GetRandom();
        }

        public abstract string RollName();

        public abstract Sprite RollPortrait();

        private int GenerateStat(Vector2 stat)
        {
            if (_rollLow)
            {
                return (int)stat.x;
            }

            return (int)Mathf.Round(Utils.MathUtils.RandomNormalBetween(stat.x, stat.y));
        }
    }
}