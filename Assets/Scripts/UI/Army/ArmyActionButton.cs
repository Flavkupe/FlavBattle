using FlavBattle.Entities;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI
{
    public class ArmyActionButton : MonoBehaviour
    {
        private IArmyAction _action;

        [SerializeField]
        [Required]
        private AudioClip _audio;

        [SerializeField]
        [Required]
        private Button _button;

        [SerializeField]
        [Required]
        private Image _icon;

        [SerializeField]
        [Required]
        private TooltipSource _tooltipSource;

        public void SetAction(IArmyAction action)
        {
            _action = action;
            _icon.sprite = action.Icon;
            _tooltipSource.TooltipText = action.TooltipMessage;
        }

        public void SetLocked(bool locked)
        {
            this._button.enabled = !locked;
        }

        public void DoAction()
        {
            if (_action != null && _button.enabled)
            {
                if (_audio != null)
                {
                    Sounds.Play(_audio);
                }

                _action.Perform();
            }
        }
    }
}
