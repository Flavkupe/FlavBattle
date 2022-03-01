using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class MiscUtils
{
    public static Vector3 MouseToWorldPoint()
    {
        var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point.z = 0;
        return point;
    }

    /// <summary>
    /// Waits until the next X frames of execution.
    /// </summary>
    public static IEnumerator WaitUntilNextFrame(int framesToWait = 1)
    {
        for (var i = 0; i < framesToWait; i++)
        {
            yield return 0;
        }
    }
    public static TType MakeOfType<TType>(string name, Transform parent = null) where TType : MonoBehaviour
    {
        var obj = new GameObject(name ?? typeof(TType).Name);
        var component = obj.AddComponent<TType>();
        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }

        return component;
    }

    public static IEnumerable<TType> FindOfType<TType>()
    {
        return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(false).OfType<TType>();
    }

    public static TResourceType[] LoadAssets<TResourceType>(string folder) where TResourceType : UnityEngine.Object
    {
        var resources = Resources.LoadAll<TResourceType>(folder);
        if (resources.Length == 0)
        {
            throw new System.Exception($"No resources found for {folder}");
        }

        var assets = string.Join<TResourceType>(", ", resources);
        
        Logger.Log(LogType.Misc, $"Loaded assets {assets}");
        return resources;
    }

    public static class MathUtils
    {
        private static System.Random rand = new System.Random();

        /// <summary>
        /// Gets an approximately normally distributed number
        /// between min and max. It's not a true distribution but
        /// it's probably good enough.
        /// 
        /// stdDev can be a bit different from 0.1 but if it's too
        /// big, results won't really be too accurate since the results
        /// are clamped between 0 and 1.
        /// </summary>
        public static float RandomNormalBetween(float min, float max, double stdDev = 0.1)
        {
            var normalish = Mathf.Clamp((float)RandomNormal(0.5, 0.1), 0.0f, 1.0f);
            return Mathf.Lerp(min, max, normalish);
        }

        /// <summary>
        /// Gets a normally distributed number with given mean and stdDev.
        /// </summary>
        public static double RandomNormal(double mean = 0, double stdDev = 1)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            // double u1 = (double)UnityEngine.Random.Range(min, max);
            // double u2 = (double)UnityEngine.Random.Range(min, max);;
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public static Vector2 RandomFurthestPointAway(Vector2 center, Vector2[] otherPoints, float radius, int trials)
        {
            var points = new List<Vector2>();
            for (var i = 0; i < trials; i++)
            {
                var randomDirection = UnityEngine.Random.insideUnitCircle * radius;
                points.Add(center + randomDirection);
            }

            var maxDist = 0.0f;
            Vector2 maxPoint = center;
            foreach (var point in points)
            {
                var sum = otherPoints.Sum(other => Vector2.Distance(point, other));
                if (sum > maxDist)
                {
                    maxPoint = point;
                    maxDist = sum;
                }
            }

            return maxPoint;
        }
    }
}
