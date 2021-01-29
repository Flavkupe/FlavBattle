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

        [Required]
        [SerializeField]
        private Image _statIcon;

        [Tooltip("Panel with layout that contains any special properties")]
        [Required]
        [SerializeField]
        private GameObject _propsPanel;

        [Tooltip("Template for icons to place on props panel.")]
        [Required]
        [SerializeField]
        private IconLabelPair _propertyTemplate;

        public void SetAction(CombatAction action, int order)
        {
            _order.text = order.ToString();
            _name.text = action.Ability.Name;

            _icon.sprite = action.Ability.Icon;

            if (action.Target.AffectsAllies())
            {
                _statIcon.Hide();
                _power.Hide();
            }
            else
            {
                _power.text = ((int)action.Ability.Damage.x).ToString();
            }

            SetProps(action);

            _grid.ResetColors();
            if (action.Target.TargetType == CombatAbilityTarget.Self)
            {
                // Do not show grid for self
                _grid.Hide();
            }
            else
            {
                var color = action.Target.AffectsAllies() ? Color.green : Color.red;
                var pairs = FormationUtils.GetFormationPairs(action.Target.ValidTargets);
                foreach (var pair in pairs)
                {
                    _grid.SetColor(pair.Row, pair.Col, color);
                }
            }
        }

        private void SetProps(CombatAction action)
        {
            _propsPanel.transform.DestroyChildren();

            // Stance requirements
            if (!action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Any))
            {
                // Only show icons if at most 2 are required (otherwise no requirement)
                if (action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Offensive))
                {
                    AddProp("Available in offensive stance", GRM.CommonSprites.OffenseIcon);
                }

                if (action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Defensive))
                {
                    AddProp("Available in defensive stance", GRM.CommonSprites.DefenseIcon);
                }

                if (action.RequiredStance.HasFlag(CombatAbilityRequiredStance.Neutral))
                {
                    AddProp("Available in neutral stance", GRM.CommonSprites.NeutralIcon);
                }
            }

            if (action.InstantAbility)
            {
                AddProp("Instant ability", GRM.CommonSprites.SandClockIcon);
            }
        }

        private void AddProp(string tooltipText, Sprite sprite)
        {
            var prop = Instantiate(_propertyTemplate, _propsPanel.transform);
            prop.SetTooltip(tooltipText);
            prop.SetIcon(sprite);
        }
    }
}