using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FlavBattle.Entities
{
    public class StatOverlay : MonoBehaviour
    {
        [SerializeField]
        [Required]
        private TextMeshPro _defenseNum;

        [SerializeField]
        [Required]
        private TextMeshPro _powerNum;

        [SerializeField]
        [Required]
        private HealthBar _healthBar;

        [SerializeField]
        [Required]
        private MoraleIcon _moraleBar;

        [SerializeField]
        private SpriteRenderer _factionFlag;

        /// <summary>
        /// Set the overlay to represent an entire army.
        /// </summary>
        public void SetArmy(ICombatArmy army)
        {
            if (_factionFlag != null)
            {
                _factionFlag.gameObject.SetActive(true);
                _factionFlag.sprite = army.Faction.Flag;
            }
        }

        public void SetUnit(Unit unit)
        {
            if (_factionFlag != null)
            {
                _factionFlag.gameObject.SetActive(false);
            }
        }

        public void UpdateOverlay(UnitStatSummary summary, Morale morale, float hpPercent)
        {
            this._powerNum.text = summary.GetTotal(UnitStatType.Power).ToString();
            this._defenseNum.text = summary.GetTotal(UnitStatType.Defense).ToString();
            this._moraleBar.UpdateIcon(morale);
            
            // no tooltip, so don't worry about actual hp, only the ratio
            this._healthBar.SetHP(0, hpPercent);
        }
    }
}