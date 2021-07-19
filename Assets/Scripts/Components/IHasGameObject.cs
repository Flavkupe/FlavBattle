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
        /// <summary>
        /// Gets the specific type of Component that the IHasGameObject
        /// returns. This happens by first calling the interface's 
        /// GetObject and then GetComponent on it, or null if it's not
        /// the right component or the object is null.
        /// </summary>
        public static T GetObject<T>(this IHasGameObject obj) where T : Component
        {
            return obj.GetObject()?.GetComponent<T>();
        }
    }
}
