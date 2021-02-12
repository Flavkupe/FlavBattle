using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FlavBattle.Triggers
{
    public class TileTrigger : Detector 
    {
        [Tooltip("Whether this is only invoked once.")]
        [SerializeField]
        private bool _oneTime;

        [SerializeField]
        private TriggerConditions _conditions;

        [SerializeField]
        public UnityEvent<Army> _armyTriggered;

        private bool _triggered = false;

        // Start is called before the first frame update
        void Start()
        {
            this.Detected += HandleDetected;
        }

        private void HandleDetected(object sender, GameObject e)
        {
            var army = e.GetComponent<Army>();
            if (army != null)
            {
                this.ProcessArmyEntered(army);
            }
        }

        private void ProcessArmyEntered(Army army)
        {
            if (_conditions.ArmyTriggers(army))
            {
                ArmyTrigger(army);
            }
        }

        private void ArmyTrigger(Army army)
        {
            if (_oneTime && _triggered)
            {
                return;
            }

            _triggered = true;
            _armyTriggered.Invoke(army);
        }
    }
}