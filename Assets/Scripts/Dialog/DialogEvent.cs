using FlavBattle.Core;
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
    }

    public abstract class MapDialogEvent : DialogEvent
    {
        public override IEnumerator DoEvent()
        {
            var cam = CameraMain.Instance;
            var sourcePos = DialogSource.position;
            yield return cam.PanTo(sourcePos);
            yield return cam.ShiftToFormationView();
            yield return new WaitForSeconds(0.5f);

            Box = CreateDialogBox();
            var shiftedSourcePos = sourcePos.ShiftY(Box.VerticalTextboxOffset);
            var offset = AdditionalDialogOffset;
            Box.transform.position = shiftedSourcePos + offset;
            Box.DialogEnd += HandleDialogEnd;
        }

        private void HandleDialogEnd(object sender, DialogBox e)
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