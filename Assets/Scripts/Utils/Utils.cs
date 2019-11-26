using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 MouseToWorldPoint()
    {
        var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point.z = 0;
        return point;
    }

    public static TType MakeOfType<TType>(string name) where TType : MonoBehaviour
    {
        var obj = new GameObject(name ?? typeof(TType).Name);
        var component = obj.AddComponent<TType>();
        return component;
    }
}
