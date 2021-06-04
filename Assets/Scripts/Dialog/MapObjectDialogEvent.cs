using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Dialog
{
    /// <summary>
    /// Generic dialog event from an object like a building
    /// </summary>
    public class MapObjectDialogEvent : MapDialogEvent
    {
        [SerializeField]
        private Sprite _portrait;

        [SerializeField]
        private string _name;

        [Tooltip("Where the dialog points to")]
        [SerializeField]
        [Required]
        private Transform _source;
        public override Transform DialogSource => _source;

        public override DialogBox CreateDialogBox()
        {
            var box = Instantiate(_dialogBoxTemplate);
            box.SetSource(_portrait, _name);
            box.SetText(Text);
            return box;
        }

        public override bool EventPossible()
        {
            return _source != null;
        }

        public override void PreStartEvent()
        {
        }
    }
}
