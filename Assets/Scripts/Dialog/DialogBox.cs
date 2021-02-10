using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField]
        [Required]
        private TextMeshPro _dialogText;

        [SerializeField]
        [Required]
        private TextMeshPro _nameText;

        [SerializeField]
        [Required]
        private SpriteRenderer _portrait;

        [SerializeField]
        [Required]
        private GameObject _downArrow;

        [Tooltip("How far to offset vertically for the box to be right above the character.")]
        [SerializeField]
        private float _verticalTextboxOffset = 1.0f;
        public float VerticalTextboxOffset => _verticalTextboxOffset;

        public event EventHandler<DialogBox> DialogEnd;

        private Queue<string> _textQueue = new Queue<string>();

        public bool Ended { get; private set; } = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                NextText();
            }
        }

        private void NextText()
        {
            if (_textQueue.Count != 0)
            {
                _dialogText.text = _textQueue.Dequeue();
            }
            else
            {
                Ended = true;
                DialogEnd?.Invoke(this, this);
            }
        }

        public void ShowDialog()
        {
            NextText();
        }

        /// <summary>
        /// Sets multiple texts for a single character.
        /// One page per text.
        /// </summary>
        public void SetText(IEnumerable<string> text)
        {
            _textQueue = new Queue<string>(text);

            // Set next to ensure we start with the first text
            NextText();
        }

        public void SetUnit(Unit unit)
        {
            this._portrait.sprite = unit.Info.Portrait;
            this._nameText.text = unit.Info.Name;
        }
    }
}