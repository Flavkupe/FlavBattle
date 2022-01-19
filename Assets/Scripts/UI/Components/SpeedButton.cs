using FlavBattle.State;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI
{
    public class SpeedButton : MonoBehaviour
    {
        [SerializeField]
        private GameSpeed _speed;

        [Required]
        [SerializeField]
        private AudioClip _toggleSound;

        [Required]
        [SerializeField]
        private Image _buttonImage;

        private bool _isToggled = false;

        public void ButtonClicked()
        {
            if (!_isToggled)
            {
                Toggle(enabled, true);
            }
        }

        public void Toggle(bool enabled, bool makeSound = false)
        {
            if (_isToggled == enabled)
            {
                return;
            }

            _isToggled = enabled;

            if (enabled)
            {
                TimeUtils.GameSpeed.SetGameSpeed(_speed);
                _buttonImage.color = _buttonImage.color.SetAlpha(0.3f);
                if (makeSound)
                {
                    SoundManager.Instance.PlayClip(_toggleSound);
                }
            }
            else
            {
                _buttonImage.color = _buttonImage.color.SetAlpha(1.0f);
            }
        }

        void Start()
        {
            Check(false);
        }

        void Update()
        {
            Check(true);
        }

        /// <summary>
        /// Check status of button vs global speed and adjust.
        /// </summary>
        /// <param name="makeSound">Whether to make a sound when toggled on.</param>
        private void Check(bool makeSound)
        {
            if (!_isToggled && TimeUtils.GameSpeed.GetGameSpeed() == _speed)
            {
                this.Toggle(true, makeSound);
            }
            else if (_isToggled && TimeUtils.GameSpeed.GetGameSpeed() != _speed)
            {
                this.Toggle(false, false);
            }
        }
    }
}
