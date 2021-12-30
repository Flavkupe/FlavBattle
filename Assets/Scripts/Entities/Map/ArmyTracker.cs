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
            public bool Visible;
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

        /// <summary>
        /// All allies in range (not just linked)
        /// </summary>
        private List<Army> _alliesInRange = new List<Army>();

        /// <summary>
        /// All enemies in range (not just flanking)
        /// </summary>
        private List<Army> _enemiesInRange = new List<Army>();

        private Army _army;

        /// <summary>
        /// Nearby allies that are actually linked (due to being close enough)
        /// </summary>
        private HashSet<TrackedArmy> _linkedArmies = new HashSet<TrackedArmy>();

        /// <summary>
        /// Nearby enemies that are actually flanking this one.
        /// </summary>
        private HashSet<TrackedArmy> _flankingArmies = new HashSet<TrackedArmy>();

        /// <summary>
        /// Gets armies that are flanking this one.
        /// </summary>
        public IEnumerable<IArmy> GetFlankingArmies()
        {
            return _flankingArmies.Select(a => a.Army);
        }

        /// <summary>
        /// Gets armies that are linked to this one.
        /// </summary>
        public IEnumerable<IArmy> GetLinkedArmies()
        {
            return _linkedArmies.Select(a => a.Army);
        }

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

            UpdateCurves(_linkedArmies);
            UpdateCurves(_flankingArmies);
        }

        private void UpdateCurves(IEnumerable<TrackedArmy> list)
        {
            foreach (var tracked in list)
            {
                if (tracked.Visible)
                {
                    // update curves
                    tracked.Curve.DrawCurveTo(tracked.Army.gameObject);
                }
            }
        }

        /// <summary>
        /// Calculates flanks based on surrounding enemies
        /// </summary>
        private void CheckFlanks()
        {
            _gizmoLines.Clear();
            if (_enemiesInRange.Count < 2)
            {
                // 2 or more enemies required for flank
                ClearAllFlanks();
                return;
            }
            
            var currentFlankingArmies = new HashSet<Army>();
            foreach (var enemy in _enemiesInRange)
            {
                foreach (var otherEnemy in _enemiesInRange)
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
            if (_flankingArmies.Count == 0)
            {
                return;
            }

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
            HideTracking(_linkedArmies);
        }

        private void HandleSelected()
        {
            foreach (var ally in _alliesInRange)
            {
                Track(_linkedArmies, ally, _allyCurveTemplate);
            }
        }

        private void Track(ICollection<TrackedArmy> list, Army army, BezierCurve curveTemplate)
        {
            var tracked = list.FirstOrDefault(a => a.Army == army);
            if (tracked != null)
            {
                tracked.Visible = true;
            }
            else
            {
                tracked = new TrackedArmy()
                {
                    Army = army,
                    Visible = true,
                };
                
                list.Add(tracked);
            }

            if (tracked.Curve == null)
            {
                var curve = Instantiate(curveTemplate, this.gameObject.transform);
                curve.transform.localPosition = Vector3.zero;
                curve.DrawCurveTo(army.gameObject);
                tracked.Curve = curve;
            }
        }

        private void StopTracking(ICollection<TrackedArmy> list, Army current)
        {
            var tracked = list.FirstOrDefault(a => a.Army == current);
            if (tracked != null)
            {
                DestroyCurve(tracked);
                list.Remove(tracked);
            }
        }

        private void HideTracking(ICollection<TrackedArmy> list)
        {
            foreach (var tracked in list)
            {
                DestroyCurve(tracked);
                tracked.Visible = false;
            }
        }

        private void DestroyCurve(TrackedArmy tracked)
        {
            if (tracked?.Curve?.gameObject != null)
            {
                Destroy(tracked.Curve.gameObject);
                tracked.Curve = null;
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
                _alliesInRange.Add(other);
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
                _alliesInRange.Remove(other);
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
                _enemiesInRange.Add(other);
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
                _enemiesInRange.Remove(other);
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
            var allies = _alliesInRange.Select(a => TraceData.ChildTraceWithContext(a.name, a.gameObject));
            var enemies = _enemiesInRange.Select(a => TraceData.ChildTraceWithContext(a.name, a.gameObject));

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
