using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Components
{
    [RequireComponent(typeof(LineRenderer))]
    public class BezierCurve : MonoBehaviour
    {
        [SerializeField]
        private GameObject _target;

        [SerializeField]
        private float _arcHeight = 1.0f;

        [SerializeField]
        private int _vertices = 100;

        private LineRenderer _line;
        private LineRenderer Line
        {
            get
            {
                if (_line == null)
                {
                    _line = GetComponent<LineRenderer>();
                }

                return _line;
            }
        }

        [ContextMenu("Draw To Target")]
        private void DrawCurveToTarget()
        {
            if (_target == null)
            {
                return;
            }

            DrawCurveTo(_target);
        }

        [ContextMenu("Clear curve")]
        public void ClearCurve()
        {
            Line.positionCount = 0;
        }

        public void DrawCurveTo(GameObject targetObj)
        {
            if (_vertices <= 0)
            {
                return;
            }

            var source = this.transform.position;
            var target = targetObj.transform.position;
            var arcPoint = (source + target) / 2.0f;
            arcPoint += Vector3.up * _arcHeight;
            var bezier = new Bezier(source, arcPoint, target);
            var numPoints = _vertices + 1;
            var points = new Vector3[numPoints];
            for (var i = 0; i < numPoints; i++)
            {
                var t = (float) i / _vertices;
                var point = bezier.GetPoint(t);
                points[i] = point.SetZ(0);
            }

            Line.positionCount = numPoints;
            Line.SetPositions(points);
        }
    }
}
