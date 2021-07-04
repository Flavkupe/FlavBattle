using FlavBattle.State;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField]
        [Required]
        private GameEventManager _gem;

        private DialogBox _box;

        void Start()
        {
            foreach (var dialogEvent in FindObjectsOfType<DialogEvent>(true))
            {
                dialogEvent.EventTriggered += TriggerDialog;
            }
        }

        public void TriggerDialog(object o, IGameEvent e)
        {
            _gem.AddOrStartGameEvent(e);
        }
    }
}
