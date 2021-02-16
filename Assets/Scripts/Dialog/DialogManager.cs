using FlavBattle.State;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        private GameEventQueue _queue;

        [SerializeField]
        [Required]
        private GameEventManager _gem;

        private DialogBox _box;

        [SerializeField]
        private KeyCode _cancelKey = KeyCode.Escape;

        private Coroutine _dialogRoutine = null;

        public event EventHandler DialogEnded;

        // Start is called before the first frame update
        void Start()
        {
            _queue = Utils.MakeOfType<GameEventQueue>("EventQueue", this.transform);
            _queue.AllDone += HandleAllEventsDone;
            _queue.SetCancelKey(_cancelKey);

            foreach (var dialogEvent in GetComponentsInChildren<DialogEvent>())
            {
                dialogEvent.EventTriggered += TriggerDialog;
            }
        }

        private void HandleAllEventsDone(object sender, EventArgs e)
        {
            _gem.TriggerMapEvent(MapEventType.MapUnpaused);
            DialogEnded?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerDialog(object o, IGameEvent e)
        {
            if (_queue.Empty)
            {
                _gem.TriggerMapEvent(MapEventType.MapPaused);
            }

            _queue.AddOrStartEvent(e);
        }
    }
}