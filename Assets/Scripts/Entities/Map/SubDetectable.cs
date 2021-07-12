using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Entities.Map
{
    /// <summary>
    /// A child component that can be used to "detect" a different component.
    /// For example, for a child component of Army to detect the Army object itself.
    /// </summary>
    public class SubDetectable : MonoBehaviour, IDetectable
    {
        [SerializeField]
        private DetectableType _type;
        public DetectableType Type => _type;

        [SerializeField]
        [Required]
        [Tooltip("The actual object this is supposed to detect")]
        private GameObject DelegatedObject;

        public GameObject GetObject()
        {
            return DelegatedObject ?? this.gameObject;
        }
    }
}
