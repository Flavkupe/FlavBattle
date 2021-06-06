using System;
using UnityEngine;
using NaughtyAttributes;
using FlavBattle.Entities.Data;
using FlavBattle.State;
using System.Linq;

namespace FlavBattle.Combat.Event
{
    public class CombatConditionalEvent : MonoBehaviour
    {
        public enum ConditionType
        {
            PlayerHasUnit,

            TurnEquals,
            
            MoraleOver,

            MoraleUnder,
        }

        public enum Criteria
        {
            AllTrue,
            AnyTrue,
        }

        public enum Team
        {
            Player,
            Other,
        }

        [Serializable]
        public class Condition
        {
            public ConditionType Type;

            public bool ShowUnit() { return Type == ConditionType.PlayerHasUnit; }
            public bool ShowValue() {
                return Type == ConditionType.TurnEquals ||
                    Type == ConditionType.MoraleUnder ||
                    Type == ConditionType.MoraleOver;
            }

            public bool ShowTeam()
            {
                return Type == ConditionType.MoraleUnder ||
                    Type == ConditionType.MoraleOver;
            }

            [ShowIf("ShowValue")]
            [AllowNesting]
            public int Value;

            [ShowIf("ShowUnit")]
            [AllowNesting]
            public UnitData Unit;

            [ShowIf("ShowTeam")]
            [AllowNesting]
            public Team Team;
        }

        [SerializeField]
        private Condition[] _conditions;

        [SerializeField]
        private Criteria _criteria;

        [SerializeField]
        private GameEventBase _event;
        public IGameEvent Event => _event;

        public bool MeetsConditions(BattleStatus state)
        {
            if (_event == null)
            {
                Debug.LogWarning("Event is empty!");
            }

            if (_criteria == Criteria.AllTrue)
            {
                // if any don't meet the condition, return false
                if (_conditions.Any(condition => !MeetsCondition(condition, state)))
                {
                    return false;
                }
            }
            else if (_criteria == Criteria.AnyTrue)
            {
                // if all don't meet the condition, return false
                if (_conditions.All(condition => !MeetsCondition(condition, state)))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MeetsCondition(Condition condition, BattleStatus state)
        {
            var army = condition.Team == Team.Player ? state.PlayerArmy : state.OtherArmy;
            if (condition.Type == ConditionType.TurnEquals && state.Round != condition.Value)
            {
                return false;
            }

            if (condition.Type == ConditionType.MoraleOver && army.Morale.Current <= condition.Value)
            {
                return false;
            }

            if (condition.Type == ConditionType.MoraleUnder && army.Morale.Current >= condition.Value)
            {
                return false;
            }

            if (condition.Type == ConditionType.PlayerHasUnit)
            {
                var units = army.GetUnits(true);
                if (!units.Any(unit => unit.SameType(condition.Unit)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
