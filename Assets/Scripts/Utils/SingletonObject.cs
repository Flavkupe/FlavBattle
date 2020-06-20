using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : class
{
    /// <summary>
    /// Gets instance of Singleton
    /// </summary>
    public static T Instance { get; private set; }

    /// <summary>
    /// Call this from Awake on the singleton, passing in "this".
    /// </summary>
    protected void SetSingleton(T instance)
    {
        if (Instance != null)
        {
            Debug.LogError($"Duplicate instance of {typeof(T)} being set!");
        }

        Instance = instance;
    }
}
