using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Components
{
    public abstract class CancellableAnimation : MonoBehaviour
    {
        [SerializeField]
        private bool _canCancel = true;

        [ShowIf("_canCancel")]
        [SerializeField]
        private KeyCode _cancelKey = KeyCode.Escape;

        [SerializeField]
        private bool _destroyOnComplete = true;

        private bool _complete = false;

        private Coroutine _coroutine = null;

        protected abstract IEnumerator DoAnimation();

        void Update()
        {
            if (_canCancel && Input.GetKey(_cancelKey))
            {
                _complete = true;
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                }
            }
        }

        public IEnumerator Animate()
        {
            _coroutine = StartCoroutine(RunAnimation());

            while (!_complete)
            {
                yield return null;
            }

            if (_destroyOnComplete)
            {
                Destroy(this.gameObject);
            }
        }

        private IEnumerator RunAnimation()
        {
            yield return DoAnimation();
            _complete = true;
        }
    }
}
