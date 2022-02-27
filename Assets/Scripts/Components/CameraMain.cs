using FlavBattle.State;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Core
{
    public class CameraMain : SingletonObject<CameraMain>
    {
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float _zoomScrollSpeed;

        [SerializeField]
        [MinMaxSlider(1.0f, 5.0f)]
        private Vector2 _zoomScrollRange;

        [SerializeField]
        private float _combatZoomDefault;

        [SerializeField]
        private float _combatZoomOutSpeed = 2.0f;

        [SerializeField]
        private float _zoomedViewSize = 2.0f;

        private bool _locked = false;

        private Camera _cam;

        public void SetLocked(bool locked)
        {
            _locked = locked;
        }

        void Awake()
        {
            _cam = this.GetComponent<Camera>();
            SetSingleton(this);
        }

        public IEnumerator ShiftToCombatZoom(float? speed = null)
        {
            yield return ZoomTo(_combatZoomDefault, speed);
        }

        /// <summary>
        /// View where units show up as formation on map.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ShiftToFormationView(float? speed = null)
        {
            yield return ZoomTo(_zoomedViewSize, speed);
        }

        private IEnumerator ZoomTo(float targetCamSize, float? speed = null)
        {
            var zoomSpeed = speed ?? _combatZoomOutSpeed;
            var wasLocked = _locked;
            SetLocked(true);
            var cam = this.GetComponent<Camera>();
            while (Mathf.Abs(targetCamSize - cam.orthographicSize) > 0.05f)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetCamSize, TimeUtils.FullAdjustedGameDelta * zoomSpeed);
                yield return null;
            }

            cam.orthographicSize = targetCamSize;

            if (!wasLocked)
            {
                SetLocked(false);
            }
        }

        void Update()
        {
            if (!_locked)
            {
                var scrollY = Input.mouseScrollDelta.y;
                if (scrollY != 0.0f)
                {
                    var cam = this.GetComponent<Camera>();
                    var size = cam.orthographicSize - (scrollY * _zoomScrollSpeed);
                    size = Mathf.Clamp(size, _zoomScrollRange.x, _zoomScrollRange.y);
                    cam.orthographicSize = size;
                }
            }
        }

        public IEnumerator PanTo(Vector3 position, float speed = 20.0f)
        {
            var wasLocked = _locked;
            SetLocked(true);
            yield return this.MoveTo(position, speed);

            if (!wasLocked)
            {
                SetLocked(false);
            }
        }

        public bool IsZoomedDistance()
        {
            return this._cam.orthographicSize <= _zoomedViewSize;
        }
    }
}