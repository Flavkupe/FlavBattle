using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlavBattle.UI
{
    public class UIAttackGrid : UIUnitGridBase
    {
        public UIFormationGridTile TileTemplate;

        protected override IFormationGridSlot OnCreateSlot()
        {
            return Instantiate(TileTemplate);
        }

        public void ResetColors()
        {
            foreach (var slot in GetSlotsAs<UIFormationGridTile>())
            {
                slot.SetColor(Color.white);
            }
        }

        public void SetColor(FormationRow row, FormationColumn col, Color color)
        {
            var slot = GetSlot<UIFormationGridTile>(row, col);
            slot.SetColor(color);
        }
    }
}