using FlavBattle.Resources;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FlavBattle.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextFromResources : MonoBehaviour
    {
        [SerializeField]
        private StringResource _textResource;

        // Start is called before the first frame update
        void Start()
        {
            var tmp = GetComponent<TextMeshProUGUI>();
            tmp.text = _textResource.Text;
        }
    }
}
