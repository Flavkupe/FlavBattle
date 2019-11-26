using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public static TResourceType[] LoadAssets<TResourceType>(string folder) where TResourceType : UnityEngine.Object
    {
        var resources = Resources.LoadAll<TResourceType>(folder);
        if (resources.Length == 0)
        {
            throw new System.Exception($"No resources found for {folder}");
        }

        var assets = string.Join<TResourceType>(", ", resources);
        
        Debug.Log($"Loaded assets {assets}");
        return resources;
    }
}
