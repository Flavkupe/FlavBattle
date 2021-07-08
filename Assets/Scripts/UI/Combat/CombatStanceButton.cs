using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{
    public class CombatStanceButton : MonoBehaviour
    {
        [SerializeField]
        [Required]
        private Image _stanceIcon;

        [SerializeField]
        [Required]
        private Image _lockIcon;

        [SerializeField]
        [Required]
        private TooltipSource _tooltipSource;

        public void SetStance(FightingStance stance)
        {
            var icon = GRM.CommonSprites.NeutralIcon;
            switch (stance)
            {
                case FightingStance.Defensive:
                    icon = GRM.CommonSprites.DefenseIcon;
                    break;
                case FightingStance.Offensive:
                    icon = GRM.CommonSprites.OffenseIcon;
                    break;
                case FightingStance.Neutral:
                default:
                    icon = GRM.CommonSprites.NeutralIcon;
                    break;
            }

            _stanceIcon.sprite = icon;

        }

        public void SetLocked(bool locked)
        {
            _lockIcon.enabled = locked;
            if (locked)
            {
                _tooltipSource.TooltipText = "Unlock stance";
            } else
            {
                _tooltipSource.TooltipText = "Lock stance";
            }
        }
    }
}
