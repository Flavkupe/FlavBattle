using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.State.Events
{
    public class SetObjectiveEvent : GameEventBase
    {
        public override bool IsAsyncEvent => false;

        public override bool IsSkippable => false;

        [SerializeField]
        private ScenarioObjective _objective;

        public override IEnumerator DoEvent()
        {
            var manager = ScenarioManager.Instance;
            if (manager == null)
            {
                Debug.LogError("Unable to get ScenarioManager; could not set new objective.");
                yield break;
            }

            yield return manager.SetAndShowObjective(_objective);
        }

        public override bool EventPossible()
        {
            return true;
        }

        public override void PreStartEvent()
        {
        }
    }
}
