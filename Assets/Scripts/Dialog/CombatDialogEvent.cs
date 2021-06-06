using FlavBattle.Combat.Event;
using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FlavBattle.Dialog
{
    public class CombatDialogEvent : DialogEvent, ICombatGameEvent
    {
        public enum SourceType
        {
            SpecificAlly,
            SpecificEnemy,
            RandomAlly,
            RandomEnemy,
        }

        public bool ShowUnitType()
        {
            return _sourceType == SourceType.SpecificAlly ||
                _sourceType == SourceType.SpecificEnemy;
        }

        private BattleStatus _battleStatus;

        [SerializeField]
        private SourceType _sourceType;

        [Tooltip("Whether the source must be fully alive to have dialog.")]
        [SerializeField]
        private bool _mustBeAlive = true;

        [ShowIf("ShowUnitType")]
        [AllowNesting]
        [SerializeField]
        private UnitData _unit;

        private CombatUnit _combatUnit;
        public override Transform DialogSource => _combatUnit?.transform;

        public override DialogBox CreateDialogBox()
        {
            var box = Instantiate(_dialogBoxTemplate);
            var unit = _combatUnit;
            if (unit == null)
            {
                return null;
            }

            box.SetUnit(unit.Unit);
            box.SetText(Text);
            return box;
        }

        public override IEnumerator DoEvent()
        {
            Box = CreateDialogBox();
            var sourcePos = DialogSource.position;
            var shiftedSourcePos = sourcePos.ShiftY(Box.VerticalTextboxOffset);
            var offset = AdditionalDialogOffset;
            Box.transform.position = shiftedSourcePos + offset;
            Box.DialogEnd += HandleDialogEnd;
            yield return null;
        }

        public override bool EventPossible()
        {
            return DialogSource != null;
        }

        public override void PreStartEvent()
        {
            _combatUnit = FindTarget();
        }

        private CombatUnit FindTarget()
        {
            if (_battleStatus == null)
            {
                Debug.LogError("No _battleStatus set for CombatDialogEvent");
                return null;
            }

            Combatant combatant = null;
            var random = false;
            IArmy army = null;
            switch (_sourceType)
            {
                case SourceType.RandomAlly:
                    random = true;
                    army = _battleStatus.PlayerArmy;
                    break;
                case SourceType.RandomEnemy:
                    random = true;
                    army = _battleStatus.OtherArmy;
                    break;
                case SourceType.SpecificAlly:
                    random = false;
                    army = _battleStatus.PlayerArmy;
                    break;
                case SourceType.SpecificEnemy:
                    random = false;
                    army = _battleStatus.OtherArmy;
                    break;
            }

            var units = army.GetUnits(_mustBeAlive);
            if (units.Count == 0)
            {
                return null;
            }

            if (!random)
            {
                if (_unit == null)
                {
                    return null;
                }

                combatant = _battleStatus.Combatants.FirstOrDefault(a => a.Unit.SameType(_unit));
            }
            else
            {
                combatant = _battleStatus.Combatants.GetRandom();
            }

            return combatant?.CombatUnit;
        }

        public void SetBattleContext(BattleStatus status)
        {
            _battleStatus = status;
        }
    }
}
