using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI.Army
{
    public class UICombatActionInfoPanel : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private UIAttackGrid _grid;

        [Required]
        [SerializeField]
        private Text _name;

        [Required]
        [SerializeField]
        private Text _order;

        [Required]
        [SerializeField]
        private Image _icon;

        [Required]
        [SerializeField]
        private Text _power;

        public void SetAction(CombatAction action, int order)
        {
            _order.text = order.ToString();
            _name.text = action.Ability.Name;

            _icon.sprite = action.Ability.Icon;

            _power.text = ((int)action.Ability.Damage.x).ToString();

            _grid.ResetColors();
            var pairs = FormationUtils.GetFormationPairs(action.Target.ValidTargets);
            foreach (var pair in pairs)
            {
                _grid.SetColor(pair.Row, pair.Col, Color.red);
            }
        }
    }
}