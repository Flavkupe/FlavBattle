using FlavBattle.Components;
using FlavBattle.Trace;
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
    public class ArmyTracker : MonoBehaviour, IHasTraceData
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

        [SerializeField]
        [Tooltip("Curve to display the enemy flanking the player")]
        [Required]
        private BezierCurve _enemyFlankCurveTemplate;

        [SerializeField]
        [Tooltip("Curve to display the player flanking the enemies")]
        [Required]
        private BezierCurve _playerFlankCurveTemplate;

        private List<Army> _allies = new List<Army>();
        private List<Army> _enemies = new List<Army>();

        private Army _army;

        private HashSet<TrackedArmy> _linkedArmies = new HashSet<TrackedArmy>();
        private HashSet<TrackedArmy> _flankingArmies = new HashSet<TrackedArmy>();

        [SerializeField]
        [Tooltip("Area for which this army can link to other allies")]
        [Required]
        private Detector _linkRange;

        [SerializeField]
        [Tooltip("Area for which this army can get flanked by enemies")]
        [Required]
        private Detector _flankRange;

        [SerializeField]
        private bool _drawGizmos = false;
        private List<GizmoLine> _gizmoLines = new List<GizmoLine>();

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

            _linkRange.Detected += HandleLinkDetected;
            _linkRange.Exited += HandleLinkExited;

            _flankRange.Detected += HandleEnemiesDetected;
            _flankRange.Exited += HandleEnemiesExited;
        }

        private void Update()
        {
            CheckFlanks();

            foreach (var tracked in _linkedArmies)
            {
                // update curves
                tracked.Curve.DrawCurveTo(tracked.Army.gameObject);
            }

            foreach (var tracked in _flankingArmies)
            {
                // update curves
                tracked.Curve.DrawCurveTo(tracked.Army.gameObject);
            }
        }

        /// <summary>
        /// Calculates flanks based on surrounding enemies
        /// </summary>
        private void CheckFlanks()
        {
            _gizmoLines.Clear();
            if (_enemies.Count < 2)
            {
                // 2 or more enemies required for flank
                ClearAllFlanks();
                return;
            }
            
            var currentFlankingArmies = new HashSet<Army>();
            foreach (var enemy in _enemies)
            {
                foreach (var otherEnemy in _enemies)
                {
                    if (enemy == otherEnemy || currentFlankingArmies.Contains(otherEnemy))
                    {
                        // already checked this
                        continue;
                    }


                    var flanking = false;
                    if (AreFlanking(enemy, otherEnemy))
                    {
                        currentFlankingArmies.Add(enemy);
                        currentFlankingArmies.Add(otherEnemy);
                        flanking = true;
                    }

                    var color = flanking ? Color.red : Color.white;
                    AddLineGizmo(enemy.transform.position, otherEnemy.transform.position, color);
                }
            }

            UpdateFlankCurves(currentFlankingArmies);
        }

        private void ClearAllFlanks()
        {
            foreach (var flank in _flankingArmies.ToList())
            {
                StopTracking(_flankingArmies, flank.Army);
            }

            _flankingArmies.Clear();
        }

        /// <summary>
        /// Updates the curves based on the current armies flanking this one
        /// </summary>
        private void UpdateFlankCurves(ICollection<Army> currentFlankingArmies)
        {
            foreach (var existing in _flankingArmies.ToList())
            {
                if (currentFlankingArmies.Contains(existing.Army))
                {
                    // already exists; update positions
                    existing.Curve.DrawCurveTo(existing.Army.gameObject);

                    // remove from here so we draw new ones as needed
                    currentFlankingArmies.Remove(existing.Army);
                }
                else
                {
                    // no longer flanking; remove from list
                    StopTracking(_flankingArmies, existing.Army);
                }
            }

            var curve = _army.IsPlayerArmy ? _enemyFlankCurveTemplate : _playerFlankCurveTemplate;
            foreach (var newFlank in currentFlankingArmies)
            {
                Track(_flankingArmies, newFlank, curve);
            }
        }

        private bool AreFlanking(Army first, Army second)
        {
            if (first == second)
            {
                return false;
            }

            var casts = Physics2D.LinecastAll(first.transform.position, second.transform.position);
            
            foreach (var cast in casts)
            {
                var detectable = cast.collider?.GetComponent<IDetectable>();
                if (detectable != null && detectable.Type == DetectableType.FlankRegion)
                {
                    var army = detectable.GetObject<Army>();
                    if (army == _army)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void HandleArmyDeselected()
        {
            foreach (var tracked in _linkedArmies)
            {
                Destroy(tracked.Curve.gameObject);
            }

            _linkedArmies.Clear();
        }

        private void HandleSelected()
        {
            if (_linkedArmies.Count > 0)
            {
                Logger.Warning(LogType.GameEvents, "Army selected twice!");
                return;
            }

            foreach (var ally in _allies)
            {
                Track(_linkedArmies, ally, _allyCurveTemplate);
            }
        }

        private void Track(ICollection<TrackedArmy> list, Army other, BezierCurve curveTemplate)
        {
            var curve = Instantiate(curveTemplate, this.gameObject.transform);
            curve.transform.localPosition = Vector3.zero;
            curve.DrawCurveTo(other.gameObject);
            list.Add(new TrackedArmy()
            {
                Army = other,
                Curve = curve
            });
        }

        private void StopTracking(ICollection<TrackedArmy> list, Army current)
        {
            var tracked = list.FirstOrDefault(a => a.Army == current);
            if (tracked != null)
            {
                Destroy(tracked.Curve.gameObject);
                list.Remove(tracked);
            }
        }

        private void HandleLinkDetected(object sender, GameObject e)
        {
            var other = e.GetComponent<Army>();
            if (other == null || other == _army)
            {
                return;
            }

            if (other.SameFaction(_army))
            {
                _allies.Add(other);
                Logger.Trace(LogType.State, "Ally entered tracked range");

                if (_army.IsSelected)
                {
                    Track(_linkedArmies, other, _allyCurveTemplate);
                }
            }
        }

        private void HandleLinkExited(object sender, GameObject e)
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

                StopTracking(_linkedArmies, other);
            }
        }

        private void HandleEnemiesDetected(object sender, GameObject e)
        {
            var other = e.GetComponent<Army>();
            if (other == null)
            {
                return;
            }

            if (!other.SameFaction(_army))
            {
                _enemies.Add(other);
                Logger.Trace(LogType.State, "Enemy entered flank range", other);
            }
        }

        private void HandleEnemiesExited(object sender, GameObject e)
        {
            var other = e.GetComponent<Army>();
            if (other == null)
            {
                return;
            }

            if (!other.SameFaction(_army))
            {
                _enemies.Remove(other);
                Logger.Trace(LogType.State, "Enemy left flank range", other);

                StopTracking(_flankingArmies, other);
            }
        }

        private void AddLineGizmo(Vector2 point1, Vector2 point2, Color color)
        {
            if (_drawGizmos)
            {
                var gizmo = new GizmoLine()
                {
                    Point1 = point1,
                    Point2 = point2,
                    Color = color,
                };

                _gizmoLines.Add(gizmo);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
            {
                return;
            }

            foreach (var gizmo in _gizmoLines)
            {
                Gizmos.color = gizmo.Color;
                Gizmos.DrawLine(gizmo.Point1, gizmo.Point2);
            }
        }

        public TraceData GetTrace()
        {
            var allies = _allies.Select(a => TraceData.ChildTraceWithContext(a.name, a.gameObject));
            var enemies = _enemies.Select(a => TraceData.ChildTraceWithContext(a.name, a.gameObject));

            var linkedArmies = new List<TraceData>();
            foreach (var item in _linkedArmies)
            {
                var trace = TraceData.ChildTrace(item.Army.name);
                trace.Context = item.Army.gameObject;
                linkedArmies.Add(trace);
            }

            var linkedTrace = TraceData.ChildTrace("Linked Armies", linkedArmies.ToArray());
            var flankingArmies = new List<TraceData>();
            foreach (var item in _flankingArmies)
            {
                var trace = TraceData.ChildTrace(item.Army.name);
                trace.Context = item.Army.gameObject;
                flankingArmies.Add(trace);
            }

            var flankTrace = TraceData.ChildTrace("Flank", flankingArmies.ToArray());
            var alliesTrace = TraceData.ChildTrace("Allies", allies.ToArray());
            var enemiesTrace = TraceData.ChildTrace("Enemies", enemies.ToArray());

            var tracked = TraceData.ChildTrace("Tracked", alliesTrace, enemiesTrace, linkedTrace, flankTrace);
            return tracked;
        }
    }
}
