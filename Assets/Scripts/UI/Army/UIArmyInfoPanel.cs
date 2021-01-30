using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FlavBattle.UI.Army
{
    /// <summary>
    /// Panel shows info about a selected army and
    /// a selected unit.
    /// </summary>
    public class UIArmyInfoPanel : MonoBehaviour
    {
        [Tooltip("The layout panel that holds individual action info panels")]
        [Required]
        [SerializeField]
        private UICombatActionInfoPanel _attackActionInfoPanel;

        [Required]
        [SerializeField]
        private UIAttackGrid _armyAttackGrid;

        public void SetArmy(IArmy army)
        {
            ColorAttackGrid(army);
        }

        public void SetUnit(Unit unit)
        {
            _attackActionInfoPanel.SetUnit(unit);
        }

        public void ClearUnit()
        {
            _attackActionInfoPanel.ClearUnit();
        }

        private void ColorAttackGrid(IArmy army)
        {
            _armyAttackGrid.ResetColors();
            var grid = new float[3,3];
            var max = 0.0f;
            foreach (var unit in army.GetUnits())
            {
                foreach (var action in unit.Info.Actions)
                {
                    if (action.Target.AffectsAllies())
                    {
                        continue;
                    }

                    var value = (int)action.Priority + 1;
                    foreach (var pair in FormationUtils.GetFormationPairs(action.Target.ValidTargets))
                    {
                        var total = grid[(int)pair.Row, (int)pair.Col] + value;
                        grid[(int)pair.Row, (int)pair.Col] = total;
                        if (total > max)
                        {
                            max = total;
                        }
                    }
                }
            }

            if (max == 0.0f)
            {
                return;
            }

            var red = Color.red;
            for (int k = 0; k < grid.GetLength(0); k++)
            {
                for (int l = 0; l < grid.GetLength(1); l++)
                {
                    var ratio = grid[k, l] / max;
                    var color = Color.Lerp(Color.white, Color.red, ratio);
                    var row = (FormationRow)k;
                    var col = (FormationColumn)l;
                    _armyAttackGrid.SetColor(row, col, color);
                }
            }
        }
    }
}