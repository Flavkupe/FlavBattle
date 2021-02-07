using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.UI.Army
{
    public class UIUnitPerksPanel : MonoBehaviour
    {
        [Tooltip("The flow panel for showing perks.")]
        [Required]
        [SerializeField]
        private GameObject _panel;

        [Tooltip("Template for displaying a perk item.")]
        [Required]
        [SerializeField]
        private UIUnitPerksPanelItem _itemTemplate;

        public void SetUnit(Unit unit)
        {
            this._panel.transform.DestroyChildren();

            if (unit == null)
            {
                return;
            }

            foreach (var perk in unit.Info.Perks)
            {
                var panelItem = Instantiate(_itemTemplate, this._panel.transform);
                panelItem.SetPerk(perk);
            }
        }
    }
}
