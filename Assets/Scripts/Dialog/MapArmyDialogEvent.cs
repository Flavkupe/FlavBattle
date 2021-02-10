using FlavBattle.Entities.Data;
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
        }

        [SerializeField]
        private EventSourceType SourceType = EventSourceType.ArmyInstance;

        private bool ShowArmyInstance() => SourceType == EventSourceType.ArmyInstance;

        [ShowIf("ShowArmyInstance")]
        [SerializeField]
        private Army _army;

        private Unit _unit;

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

        public override void Init()
        {
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

        public override bool DialogPossible()
        {
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
    }
}
