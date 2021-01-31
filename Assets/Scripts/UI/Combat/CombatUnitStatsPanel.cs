using FlavBattle.UI.Army;
using FlavBattle.UI.Combat;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{
    public class CombatUnitStatsPanel : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private IconLabelPair _hpLabel;

        [Required]
        [SerializeField]
        private MoraleIcon _moraleIcon;

        [Required]
        [SerializeField]
        private TextMeshProUGUI _moraleText;

        [Required]
        [SerializeField]
        private Text _nameLabel;

        [Required]
        [SerializeField]
        private Image _unitSprite;

        [Required]
        [SerializeField]
        private UICombatActionInfoPanel _actionInfoPanel;

        [Required]
        [SerializeField]
        private UnitStatSummaryPanel _attackSummaryPanel;

        [Required]
        [SerializeField]
        private UnitStatSummaryPanel _defenseSummaryPanel;

        void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                this.Close();
            }
        }

        public void SetCombatant(Combatant combatant)
        {
            _unitSprite.sprite = combatant.Unit.Data.Icon;
            _nameLabel.text = combatant.Unit.Info.Name;

            var currHp = combatant.Unit.Info.CurrentStats.HP;
            var maxHp = combatant.Unit.Info.MaxStats.HP;
            _hpLabel.SetText($"{currHp}/{maxHp}");

            _moraleText.text = combatant.UnitMorale.Current.ToString();
            _moraleIcon.UpdateIcon(combatant.UnitMorale);

            _actionInfoPanel.SetUnit(combatant.Unit);

            _attackSummaryPanel.SetSummary(combatant.Unit.StatSummary);
            _defenseSummaryPanel.SetSummary(combatant.Unit.StatSummary);
        }

        public void Close()
        {
            TimeUtils.GameSpeed.Unpause();
            this.gameObject.Hide();
        }

        public void Open(Combatant combatant)
        {
            TimeUtils.GameSpeed.Pause();
            this.Show();
            SetCombatant(combatant);
        }
    }
}