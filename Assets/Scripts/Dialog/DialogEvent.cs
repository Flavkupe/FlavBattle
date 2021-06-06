using FlavBattle.State;
using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public abstract class DialogEvent : GameEventBase, IGameEvent
    {
        [SerializeField]
        [Required]
        protected DialogBox _dialogBoxTemplate;

        protected DialogBox Box { get; set;  }

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

        protected void HandleDialogEnd(object sender, DialogBox e)
        {
            // TEMP?
            Destroy(e.gameObject);
            Box = null;
            InvokeEventFinished();
        }

        public override void CancelEvent()
        {
            base.CancelEvent();

            if (Box != null)
            {
                Destroy(Box.gameObject);
                Box = null;
            }
        }
    }
}
