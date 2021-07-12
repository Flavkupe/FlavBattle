using FlavBattle.Components;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Map
{
    /// <summary>
    /// Keeps track of nearby armies using detectors.
    /// </summary>
    [RequireComponent(typeof(Detector))]
    public class ArmyTracker : MonoBehaviour
    {
        private class TrackedArmy
        {
            public Army Army;
            public BezierCurve Curve;
        }

        [SerializeField]
        [Tooltip("Curve to use for pointing to nearby allies")]
        [Required]
        private BezierCurve _allyCurveTemplate;

        private List<Army> _allies = new List<Army>();

        private Army _army;

        private List<TrackedArmy> _trackedArmies = new List<TrackedArmy>();

        private void Start()
        {
            _army = GetComponentInParent<Army>();
            if (_army == null)
            {
                Logger.Error(LogType.Init, "ArmyTracker not attached to an army!");
                return;
            }

            _army.Selected += HandleSelected;
            _army.Deselected += HandleArmyDeselected;

            var detector = GetComponent<Detector>();
            if (detector == null)
            {
                Logger.Error(LogType.Init, "No detector for ArmyTracker!");
                return;
            }

            detector.Detected += HandleDetected;
            detector.Exited += HandleExited;
        }

        private void Update()
        {
            foreach (var tracked in _trackedArmies)
            {
                // update curves
                tracked.Curve.DrawCurveTo(tracked.Army.gameObject);
            }
        }

        private void HandleArmyDeselected()
        {
            foreach (var tracked in _trackedArmies)
            {
                Destroy(tracked.Curve.gameObject);
            }

            _trackedArmies.Clear();
        }

        private void HandleSelected()
        {
            if (_trackedArmies.Count > 0)
            {
                Logger.Warning(LogType.GameEvents, "Army selected twice!");
                return;
            }

            foreach (var ally in _allies)
            {
                Track(ally);
            }
        }

        private void Track(Army other)
        {
            var curve = Instantiate(_allyCurveTemplate, this.gameObject.transform);
            curve.transform.localPosition = Vector3.zero;
            curve.DrawCurveTo(other.gameObject);
            _trackedArmies.Add(new TrackedArmy()
            {
                Army = other,
                Curve = curve
            });
        }

        private void HandleDetected(object sender, GameObject e)
        {
            var other = e.GetComponent<Army>();
            if (other == null)
            {
                return;
            }

            if (other.SameFaction(_army))
            {
                _allies.Add(other);
                Logger.Trace(LogType.State, "Ally entered tracked range");

                if (_army.IsSelected)
                {
                    Track(other);
                }
            }
        }

        private void HandleExited(object sender, GameObject e)
        {
            var other = e.GetComponent<Army>();
            if (other == null)
            {
                return;
            }

            if (other.SameFaction(_army))
            {
                _allies.Remove(other);
                Logger.Trace(LogType.State, "Ally no longer in range");
            }

            var tracked = _trackedArmies.FirstOrDefault(a => a.Army == other);
            if (tracked != null)
            {
                Destroy(tracked.Curve.gameObject);
                _trackedArmies.Remove(tracked);
            }
        }
    }
}
