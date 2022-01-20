using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.Entities
{
    public class ArmyActions : MonoBehaviour
    {
        [Serializable]
        public class DefaultIcons
        {
            public Sprite Garrison;
        }

        [SerializeField]
        private DefaultIcons _defaultIcons;

        [SerializeField]
        [Required]
        private Army _army;

        private List<IArmyAction> _actions = new List<IArmyAction>();
        public List<IArmyAction> Actions => _actions;

        private ActionButtonsPanel Panel
        {
            get
            {
                var ui = Instances.Current?.Managers?.UI;
                if (ui == null)
                {
                    Debug.LogWarning("Could not find action button panel!");
                    return null;
                }

                return ui.ActionButtonsPanel;
            }
        }

        void Awake()
        {
            if (_army == null)
            {
                Debug.LogError("No army set for ArmyActions!");
                return;
            }

            SetArmy(_army);
        }

        private void SetArmy(Army army)
        {
            _army = army;
            _actions.Clear();
            // _army.Selected += HandleArmy_Selected;
            // _army.Deselected += HandleArmy_Deselected;

            // default actions
            this.Actions.Add(new GarrisonAction(this._defaultIcons.Garrison));

            foreach (var action in this.Actions)
            {
                action.SetArmy(_army);
            }
        }

        //private void HandleArmy_Deselected()
        //{
        //    Panel?.UnsetArmyActions(this);
        //}

        //private void HandleArmy_Selected()
        //{
        //    Panel?.SetArmyActions(this);
        //}
    }
}
