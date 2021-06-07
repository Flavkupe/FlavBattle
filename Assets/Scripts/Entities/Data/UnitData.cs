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

        [Tooltip("ID to identify this special unit or basic class uniquely.")]
        [SerializeField]
        [ValidateInput("NonNullID", "UnitID must be filled")]
        private string _unitID;
        public string UnitID => _unitID;

        private bool NonNullID() => !string.IsNullOrWhiteSpace(_unitID);

        [SerializeField]
        private string _className;
        public virtual string ClassName => _className;

        [Tooltip("Debug option to always use lowest roll for stat rolls.")]
        [SerializeField]
        protected bool _rollLow = false;

        [BoxGroup("Base Stats")]
        [SerializeField]
        private int _hp;
        public int HP => _hp;

        [BoxGroup("Base Stats")]
        [SerializeField]
        private int _power;
        public int Power => _power;

        [BoxGroup("Base Stats")]
        [SerializeField]
        private int _defense;
        public int Defense => _defense;

        [BoxGroup("Base Stats")]
        [SerializeField]
        private int _speed;
        public int Speed => _speed;

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
        private RaceData _race;
        public virtual RaceData Race => _race;

        [BoxGroup("Abilities")]
        [SerializeField]
        [ReorderableList]
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

        [BoxGroup("Abilities")]
        [Tooltip("Perks available by level")]
        [SerializeField]
        [ReorderableList]
        private PerksByLevel[] _perksByLevel;
        public PerksByLevel[] PerksByLevel => _perksByLevel;

        /// <summary>
        /// Roll unit stats from the possible base props
        /// </summary>
        /// <returns></returns>
        public virtual UnitStats RollStartingStats(int level)
        {
            var stats = new UnitStats();
            stats.Level = level;
            stats.HP = HP;
            stats.Power = Power;
            stats.Defense = Defense;
            stats.Speed = Speed;
            stats.StartingBlockShields = StartingBlockShields;
            for (int i = 1; i < level; i++)
            {
                var levelup = RollLevel();
                stats.Combine(levelup);
            }

            return stats;
        }

        public virtual UnitStats RollLevel()
        {
            // TODO - perks
            var stats = new UnitStats();
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

        public virtual PerkData RollPerk(int level)
        {
            var perkInfo = this.PerksByLevel.FirstOrDefault(a => a.Level == level);
            if (perkInfo != null && perkInfo.Perks.Length > 0)
            {
                return perkInfo.Perks.GetRandom();
            }

            return null;
        }

        public abstract string RollName();

        public abstract Sprite RollPortrait();
    }

    [Serializable]
    public class PerksByLevel
    {
        public int Level;

        [ReorderableList]
        [AllowNesting]
        public PerkData[] Perks;
    }
}