using UnityEngine;

namespace FlavBattle.Components
{
    /// <summary>
    /// An object backed by a GameObject.
    /// </summary>
    public interface IHasGameObject
    {
        GameObject GetObject();
    }

    public static class IHasGameObjectExtensions
    {
        public static T GetComponent<T>(this IHasGameObject obj) where T : Component
        {
            return obj.GetObject()?.GetComponent<T>();
        }
    }
}
