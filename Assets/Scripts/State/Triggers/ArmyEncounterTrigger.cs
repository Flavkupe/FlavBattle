using FlavBattle.Dialog;
using FlavBattle.State;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace FlavBattle.Triggers
{
    public class ArmyEncounterTrigger : MonoBehaviour
    {
        [SerializeField]
        private TriggerConditions _conditions;

        [ShowIf("ShowTriggerEvent")]
        [SerializeField]
        private UnityEvent<Army> _armyTriggered;

        [Tooltip("If this is set, will fire this event through GameEventManager on trigger")]
        [SerializeField]
        private GameEventBase _gameEventOnTrigger;

        private bool ShowTriggerEvent() => _gameEventOnTrigger == null;

        private GameEventManager _gem;

        void Awake()
        {
            _gem = FindObjectOfType<GameEventManager>(true);
        }

        public bool ArmyFullfillsCondition(Army army)
        {
            return _conditions.ArmyTriggers(army);
        }

        public void TriggerWithArmy(Army army)
        {
            if (_gameEventOnTrigger != null)
            {
                var dialog = _gameEventOnTrigger.GetComponent<MapArmyDialogEvent>();
                if (dialog != null)
                {
                    // TODO: can I improve this code...?
                    // Specific army case
                    dialog.TriggerWithArmy(army);
                }
                else
                {
                    _gem.AddOrStartGameEvent(_gameEventOnTrigger);
                }
            }
            else
            {
                _armyTriggered?.Invoke(army);
            }
        }
    }
}
