using FlavBattle.Core;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public abstract class MapDialogEvent : DialogEvent
    {
        public override IEnumerator DoEvent()
        {
            var cam = CameraMain.Instance;
            var sourcePos = DialogSource.position;
            yield return cam.PanTo(sourcePos);
            yield return cam.ShiftToFormationView();
            yield return new WaitForSeconds(0.5f);

            Box = CreateDialogBox();
            var shiftedSourcePos = sourcePos.ShiftY(Box.VerticalTextboxOffset);
            var offset = AdditionalDialogOffset;
            Box.transform.position = shiftedSourcePos + offset;
            Box.DialogEnd += HandleDialogEnd;
        }
    }
}
