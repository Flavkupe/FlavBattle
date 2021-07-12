using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Bezier
{
    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;

    public Bezier(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public Vector3 GetPoint(float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1.0f - t;
        return oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }

    public Vector3 GetFirstDerivative(float t)
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }

    public Vector3 GetVelocity(Transform transform, float t)
    {
        return transform.TransformPoint(this.GetFirstDerivative(t)) - transform.position;
    }

    public Vector3 GetDirection(Transform transform, float t)
    {
        return GetVelocity(transform, t).normalized;
    }

    public Vector3 GetDirection(float t)
    {
        return GetFirstDerivative(t).normalized;
    }
}
