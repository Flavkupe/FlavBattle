using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FlavBattle.State
{
    public class GenericGameEvent : GameEventBase
    {
        public override bool IsAsyncEvent => true;

        [SerializeField]
        private UnityEvent _action;

        [SerializeField]
        private float _waitBeforeSeconds = 0.0f;

        [SerializeField]
        private float _waitAfterSeconds = 0.0f;

        public override IEnumerator DoEvent()
        {
            if (_waitBeforeSeconds > 0.0f)
            {
                yield return new WaitForSeconds(_waitBeforeSeconds);
            }

            _action.Invoke();

            if (_waitAfterSeconds > 0.0f)
            {
                yield return new WaitForSeconds(_waitAfterSeconds);
            }
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
