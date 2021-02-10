using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public abstract class DialogEvent : MonoBehaviour
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

        [SerializeField]
        private DialogEvent _followupEvent;
        public DialogEvent FollowupEvent => _followupEvent;

        /// <summary>
        /// Fires when this event is ready to invoke; queues up
        /// this event.
        /// </summary>
        public event EventHandler<DialogEvent> TriggerDialog;

        private bool _started = false;

        public abstract DialogBox CreateDialogBox();

        /// <summary>
        /// Whether or not this dialog event is possible
        /// (for example, if a needed character is alive).
        /// </summary>
        public abstract bool DialogPossible();

        /// <summary>
        /// An extra offset from the source for which to offset the
        /// dialog box.
        /// </summary>
        public Vector3 AdditionalDialogOffset { get; protected set; }

        /// <summary>
        /// Initializes the event, such as by looking for relevant objects
        /// in map.
        /// </summary>
        public abstract void Init();

        [ContextMenu("Start Event")]
        public void StartEvent()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            TriggerDialog?.Invoke(this, this);
        }
    }
}