using System.Collections;
using UnityEngine;

namespace FlavBattle.Utils
{
    /// <summary>
    /// Alternative to CancellableRoutine which should be easier to use.
    /// </summary>
    public class CancellableCoroutineContainer
    {
        private bool _done = false;

        private Coroutine _coroutine = null;
        private IEnumerator _routine = null;
        private MonoBehaviour _initiator;

        public bool Skipped { get; private set; }

        public CancellableCoroutineContainer(MonoBehaviour initiator, IEnumerator routine)
        {
            _initiator = initiator;
            _routine = routine;
        }

        public IEnumerator Do()
        {
            _done = false;
            _coroutine = _initiator.StartCoroutine(DoCancellable());
            _initiator.StartCoroutine(ListenForSkip());
            while (!_done)
            {
                yield return null;
            }

            _coroutine = null;
        }

        private IEnumerator DoCancellable()
        {
            yield return _routine;
            _done = true;
        }

        private IEnumerator ListenForSkip()
        {
            while (!_done && _coroutine != null)
            {
                // TODO: mappable key
                if (Input.GetKey(KeyCode.Escape) && _coroutine != null)
                {
                    Skipped = true;
                    _done = true;
                    _initiator.StopCoroutine(_coroutine);
                    _coroutine = null;
                }

                yield return null;
            }

            _done = true;
        }
    }
}
