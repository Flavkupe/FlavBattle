using FlavBattle.Entities;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{
    public class UnitStatSummaryPanelRow : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private Text _valueText;

        [Required]
        [SerializeField]
        private Text _descriptionText;

        public void SetSummaryItem(UnitStatSummary.SummaryItem summaryItem)
        {
            var amt = summaryItem.Amount;
            _valueText.text = amt > 0 ? ("+" + amt) : amt.ToString();
            _descriptionText.text = summaryItem.Description;
        }
    }
}
