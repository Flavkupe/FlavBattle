using FlavBattle.Core;
using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Dialog
{
    /// <summary>
    /// Creates dialog from an army in the map.
    /// </summary>
    public class MapArmyDialogEvent : DialogEvent
    {
        [Tooltip("Matching UnitData to have the dialog. If null, will be random unit in army.")]
        [SerializeField]
        private UnitData _character;

        public enum EventSourceType
        {
            ArmyInstance,
            FirstArmyMatchingUnit,

            // Event provides the params (via unity event)
            TriggeredExternally,
        }

        [SerializeField]
        private EventSourceType SourceType = EventSourceType.ArmyInstance;

        private bool ShowArmyInstance() => SourceType == EventSourceType.ArmyInstance;

        [ShowIf("ShowArmyInstance")]
        [SerializeField]
        private Army _army;

        private Unit _unit;

        private DialogBox _box;

        private CameraMain _cam;

        public override Transform DialogSource => _army?.transform;

        public override DialogBox CreateDialogBox()
        {
            var box = Instantiate(_dialogBoxTemplate);
            var unit = FindMatchingUnit();
            if (unit == null)
            {
                return null;
            }

            box.SetUnit(unit);
            box.SetText(Text);
            return box;
        }

        public override void PreStartEvent()
        {
            _cam = CameraMain.Instance;

            if (SourceType == EventSourceType.FirstArmyMatchingUnit)
            {
                var armies = FindObjectsOfType<Army>(false);
                _army = armies.FirstOrDefault(a => GetUnitFromArmy(a) != null);
            }

            _unit = FindMatchingUnit();

            if (_army != null && _unit != null)
            {
                // find offset
                var sprite = _army.GetSpriteAtFormationPair(_unit.Formation);
                if (sprite != null)
                {
                    AdditionalDialogOffset = sprite.transform.position - DialogSource.position;
                }
            }
        }

        public void TriggerWithArmy(Army army)
        {
            _unit = null;
            _army = army;
            TriggerEvent();
        }

        public override bool EventPossible()
        {
            if (IsCompleted)
            {
                // already was triggered before
                return false;
            }

            if (DialogSource == null)
            {
                Debug.LogWarning("DialogSource null for event");
                return false;
            }

            // possible as long as needed army unit is not null
            return FindMatchingUnit() != null;
        }

        private Unit FindMatchingUnit()
        {
            if (_unit != null)
            {
                // cached
                return _unit;
            }

            if (_army == null || _army.gameObject == null)
            {
                // TODO: destroyed army?
                return null;
            }

            _unit = GetUnitFromArmy(_army);
            return _unit;
        }

        private Unit GetUnitFromArmy(Army army)
        {
            if (_character == null)
            {
                return army.GetUnits(true).GetRandom();
            }
            else
            {
                return army.GetUnits(true).FirstOrDefault(a => a.Data.UnitID == _character.UnitID);
            }
        }

        public override IEnumerator DoEvent()
        {
            var sourcePos = DialogSource.position;
            yield return _cam.PanTo(sourcePos);
            yield return _cam.ShiftToFormationView();
            yield return new WaitForSeconds(0.5f);

            _box = CreateDialogBox();
            var shiftedSourcePos = sourcePos.ShiftY(_box.VerticalTextboxOffset);
            var offset = AdditionalDialogOffset;
            _box.transform.position = shiftedSourcePos + offset;
            _box.DialogEnd += HandleDialogEnd;
        }

        public override void CancelEvent()
        {
            base.CancelEvent();

            if (_box != null)
            {
                Destroy(_box.gameObject);
                _box = null;
            }
        }

        private void HandleDialogEnd(object sender, DialogBox e)
        {
            // TEMP?
            Destroy(e.gameObject);
            _box = null;
            InvokeEventFinished();
        }
    }
}
