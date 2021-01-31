using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlavBattle.Entities;
using UnityEngine.UI;

namespace FlavBattle.UI.Combat
{

    public class UnitStatSummaryPanel : MonoBehaviour
    {
        [Tooltip("What stat this panel is for.")]
        [SerializeField]
        private UnitStatSummary.SummaryItemType _statType;

        [Required]
        [SerializeField]
        private UnitStatSummaryPanelRow _summaryRowTemplate;

        [Tooltip("The layout panel where items are layed out.")]
        [Required]
        [SerializeField]
        private GameObject _layoutPanel;

        [Tooltip("The text showing the total")]
        [Required]
        [SerializeField]
        private Text _totalText;

        public void SetSummary(UnitStatSummary summary)
        {
            _layoutPanel.transform.DestroyChildren();
            foreach (var item in summary.Items)
            {
                if (item.Type == _statType)
                {
                    var row = Instantiate(_summaryRowTemplate, this._layoutPanel.transform);
                    row.SetSummaryItem(item);
                }
            }

            _totalText.text = $"Total: {summary.GetTotal(_statType)}";
        }
    }
}