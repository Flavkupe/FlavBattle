using FlavBattle.Core;
using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        private Queue<DialogEvent> _dialogQueue = new Queue<DialogEvent>();

        private DialogEvent _currentdialog = null;

        [Tooltip("Camera is required to pan to location for dialog.")]
        [Required]
        [SerializeField]
        private CameraMain _cam;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var dialogEvent in GetComponentsInChildren<DialogEvent>())
            {
                dialogEvent.TriggerDialog += HandleTriggerDialog;
            }
        }

        private void HandleTriggerDialog(object sender, DialogEvent e)
        {
            if (_currentdialog != null)
            {
                _dialogQueue.Enqueue(e);
            }
            else
            {
                StartCoroutine(ShowDialog(e));
            }
        }

        private IEnumerator ShowDialog(DialogEvent nextEvent)
        {
            _currentdialog = nextEvent;
            _currentdialog.Init();
            if (!_currentdialog.DialogPossible() || _currentdialog.DialogSource == null)
            {
                NextDialog();
                yield break;
            }


            var sourcePos = _currentdialog.DialogSource.position;
            yield return _cam.PanTo(sourcePos);
            yield return _cam.ShiftToFormationView();
            yield return new WaitForSeconds(0.5f);

            var box = nextEvent.CreateDialogBox();
            var shiftedSourcePos = sourcePos.ShiftY(box.VerticalTextboxOffset);
            var offset = nextEvent.AdditionalDialogOffset;
            box.transform.position = shiftedSourcePos + offset;
            box.DialogEnd += HandleDialogEnd;
        }

        private void HandleDialogEnd(object sender, DialogBox e)
        {
            // TEMP?
            Destroy(e.gameObject);
            NextDialog();
        }

        private void NextDialog()
        {
            if (_currentdialog != null && _currentdialog.FollowupEvent != null)
            {
                // followups available
                StartCoroutine(ShowDialog(_currentdialog.FollowupEvent));
            }
            else if (_dialogQueue.Count > 0)
            {
                StartCoroutine(ShowDialog(_dialogQueue.Dequeue()));
            }
            else
            {
                // no more dialog
                _currentdialog = null;
            }
        }
    }
}