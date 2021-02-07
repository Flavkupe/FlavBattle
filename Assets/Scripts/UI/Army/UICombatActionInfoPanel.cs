using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlavBattle.UI.Army
{
    public class UICombatActionInfoPanel : MonoBehaviour
    {
        [Tooltip("Template for a panel that holds info about a single action")]
        [Required]
        [SerializeField]
        private UICombatActionInfoItem _UICombatActionInfoPanelTemplate;

        public void SetUnit(Unit unit)
        {
            ClearUnit();
            var count = 1;
            foreach (var action in unit.Info.Actions)
            {
                var panel = Instantiate(_UICombatActionInfoPanelTemplate, this.transform);
                panel.SetAction(action, count);
                count++;
            }
        }

        public void ClearUnit()
        {
            this.transform.DestroyChildren();
        }
    }
}