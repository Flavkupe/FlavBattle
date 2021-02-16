using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Entities.Map
{
    /// <summary>
    /// A child component that can be used to "detect" a parent component
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
