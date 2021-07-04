using FlavBattle.Components;
using FlavBattle.Core;
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
    public class PanAndIndicateEvent : GameEventBase
    {
        [SerializeField]
        [Required]
        private GameObject _target;

        [SerializeField]
        private AnimatedBounce _bounceTemplate;

        [SerializeField]
        [Tooltip("How far up the bounce indicator will be.")]
        private float _verticalOffset = 0.5f;

        public override bool IsAsyncEvent => false;

        public override bool IsSkippable => false;

        public override IEnumerator DoEvent()
        {
            var cam = CameraMain.Instance;
            if (cam == null)
            {
                yield break;
            }

            var pos = _target.transform.position;
            yield return cam.PanTo(pos);

            if (_bounceTemplate != null)
            {
                var bounce = Instantiate(_bounceTemplate);
                bounce.transform.position = _target.transform.position.ShiftY(_verticalOffset);
                yield return bounce.Animate();
            }
        }

        public override bool EventPossible()
        {
            return _target != null;
        }

        public override void PreStartEvent()
        {
        }
    }
}
