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
    public class DialogManager : MonoBehaviour
    {
        private Queue<DialogEvent> _dialogQueue = new Queue<DialogEvent>();

        private DialogEvent _currentdialog = null;

        [Tooltip("Camera is required to pan to location for dialog.")]
        [Required]
        [SerializeField]
        private CameraMain _cam;

        [SerializeField]
        [Required]
        private GameEventManager _gem;

        private DialogBox _box;

        [SerializeField]
        private KeyCode _cancelKey = KeyCode.Escape;

        private Coroutine _dialogRoutine = null;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var dialogEvent in GetComponentsInChildren<DialogEvent>())
            {
                dialogEvent.TriggerDialog += HandleTriggerDialog;
            }
        }

        void Update()
        {
            if (Input.GetKeyUp(_cancelKey))
            {
                AllDialogDone();
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
                StartDialogRoutine(e);
            }
        }

        private void StartDialogRoutine(DialogEvent e)
        {
            _dialogRoutine = StartCoroutine(ShowDialog(e));
        }

        private IEnumerator ShowDialog(DialogEvent nextEvent)
        {
            _currentdialog = nextEvent;
            _currentdialog.Init();
            if (!_currentdialog.DialogPossible() || _currentdialog.DialogSource == null)
            {
                NextDialog();
                _dialogRoutine = null;
                yield break;
            }

            _gem.TriggerMapEvent(MapEventType.MapPaused);
            var sourcePos = _currentdialog.DialogSource.position;
            yield return _cam.PanTo(sourcePos);
            yield return _cam.ShiftToFormationView();
            yield return new WaitForSeconds(0.5f);

            _box = nextEvent.CreateDialogBox();
            var shiftedSourcePos = sourcePos.ShiftY(_box.VerticalTextboxOffset);
            var offset = nextEvent.AdditionalDialogOffset;
            _box.transform.position = shiftedSourcePos + offset;
            _box.DialogEnd += HandleDialogEnd;
            _dialogRoutine = null;
        }

        private void HandleDialogEnd(object sender, DialogBox e)
        {
            // TEMP?
            Destroy(e.gameObject);
            _box = null;
            NextDialog();
        }

        private void AllDialogDone()
        {
            if (_dialogRoutine != null)
            {
                StopCoroutine(_dialogRoutine);
                _dialogRoutine = null;
            }

            if (_box != null)
            {
                Destroy(_box.gameObject);
                _box = null;
            }

            _currentdialog = null;
            _gem.TriggerMapEvent(MapEventType.MapUnpaused);
        }

        private void NextDialog()
        {
            if (_currentdialog != null && _currentdialog.FollowupEvent != null)
            {
                // followups available
                StartDialogRoutine(_currentdialog.FollowupEvent);
            }
            else if (_dialogQueue.Count > 0)
            {
                StartDialogRoutine(_dialogQueue.Dequeue());
            }
            else
            {
                // no more dialog
                AllDialogDone();
            }
        }
    }
}