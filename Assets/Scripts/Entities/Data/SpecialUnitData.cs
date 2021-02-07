using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Data
{
    [CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Special Unit Data", order = 1)]
    public class SpecialUnitData : UnitData
    {
        /// <summary>
        /// Just for showing the assetIcon in the inspector; ignore this
        /// </summary>
        [AssetIcon]
        public Sprite AssetIcon => base.Icon;

        [BoxGroup("Overrides")]
        [SerializeField]
        private int _startingLevel = 1;

        [BoxGroup("Overrides")]
        [SerializeField]
        private string _unitName;

        [BoxGroup("Overrides")]
        [Required]
        [SerializeField]
        private Sprite _portrait;

        [Tooltip("This unit's stats are combined with the base stat rolls")]
        [Required]
        [SerializeField]
        private BasicUnitData _baseData;

        public override CombatAction[] StartingActions => base.StartingActions.Concat(_baseData.StartingActions).ToArray();
        public override AnimatorOverrideController Animator => GetNonNull(base.Animator, _baseData.Animator); 
        public override Sprite Icon => GetNonNull(base.Icon, _baseData.Icon);
        public override string ClassName => GetNonEmpty(base.ClassName, _baseData.ClassName);
        public override Sprite[] Animations => GetNonEmpty(base.Animations, _baseData.Animations);
        

        public override UnitStats RollLevel()
        {
            return base.RollLevel().GetCombined(_baseData.RollLevel());
        }

        public override string RollName()
        {
            return _unitName;
        }

        /// <summary>
        /// Uses the left if non-empty (overridden). Otheriwse uses the right.
        /// </summary>
        private string GetNonEmpty(string left, string right)
        {
            return string.IsNullOrEmpty(left) ? right : left;
        }

        /// <summary>
        /// Uses the left if non-empty (overridden). Otheriwse uses the right.
        /// </summary>
        private T[] GetNonEmpty<T>(T[] left, T[] right)
        {
            return left.Length == 0 ? right : left;
        }

        /// <summary>
        /// Uses the left if non-null (overridden). Otheriwse uses the right.
        /// </summary>
        private T GetNonNull<T>(T left, T right) where T : class
        {
            return left == null ? right : left;
        }

        public override OfficerAbilityData RollNewOfficerAbility(int level, List<OfficerAbilityData> existing)
        {
            var ability = base.RollNewOfficerAbility(level, existing);
            if (ability != null)
            {
                return ability;
            }

            return _baseData.RollNewOfficerAbility(level, existing);
        }

        public override Sprite RollPortrait()
        {
            return _portrait;
        }

        /// <summary>
        /// Prefer special type perks but apply base class otherwise.
        /// </summary>
        public override PerkData RollPerk(int level)
        {
            return GetNonNull(base.RollPerk(level), _baseData.RollPerk(level));
        }

        public override UnitStats RollStartingStats(int level)
        {
            level = Math.Max(level, _startingLevel);
            return base.RollStartingStats(level).GetCombined(_baseData.RollStartingStats(level));
        }
    }
}
