using System.Collections;
using UnityEngine;

namespace FlavBattle.Components
{
    public class AnimatedBounce : CancellableAnimation
    {
        [SerializeField]
        private float _range = 0.5f;

        [SerializeField]
        private float _speed = 1.0f;

        [SerializeField]
        [Tooltip("If true, will start bouncing on active.")]
        private bool _autoStart = false;

        [SerializeField]
        [Tooltip("How many times to bounce, or 0 to bounce forever (careful!)")]
        private int _cycles = 2;

        private float _period = 0.0f;

        private int _current_cycle = 0;

        const float TWO_PI = 2.0f * Mathf.PI;

        private void Start()
        {
            if (_autoStart)
            {
                StartCoroutine(DoAnimation());
            }
        }

        protected override IEnumerator DoAnimation()
        {
            var centerY = this.transform.position.y;
            while (_cycles == 0 || _current_cycle < _cycles)
            {
                var tick = _speed * Time.deltaTime;
                _period += tick;
                var y = centerY + Mathf.Cos(_period) * _range;
                this.transform.position = this.transform.position.SetY(y);
                if (_period > TWO_PI)
                {
                    _period = _period % TWO_PI;
                    _current_cycle++;
                }

                yield return null;
            }
        }
    }
}
