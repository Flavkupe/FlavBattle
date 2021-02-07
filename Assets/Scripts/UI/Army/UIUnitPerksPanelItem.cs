using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Army
{
    public class UIUnitPerksPanelItem : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private TextMeshProUGUI _description;

        [Required]
        [SerializeField]
        private Text _name;

        [Required]
        [SerializeField]
        private Image _icon;

        public void SetPerk(PerkData data)
        {
            _description.text = data.Description;
            _name.text = data.Name;
            _icon.sprite = data.Icon;
        }
    }
}
