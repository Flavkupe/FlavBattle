using FlavBattle.Entities.Data;
using FlavBattle.State;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public abstract class DialogEvent : GameEventBase, IGameEvent
    {
        [SerializeField]
        [Required]
        protected DialogBox _dialogBoxTemplate;

        /// <summary>
        /// Where the dialog comes from (what to center on)
        /// </summary>
        public abstract Transform DialogSource { get; }

        [SerializeField]
        private string[] _text;
        public string[] Text => _text;

        public abstract DialogBox CreateDialogBox();

        /// <summary>
        /// An extra offset from the source for which to offset the
        /// dialog box.
        /// </summary>
        public Vector3 AdditionalDialogOffset { get; protected set; }
    }
}