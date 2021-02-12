using FlavBattle.Entities.Data;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlavBattle.Triggers
{
    public interface ITriggerCondition
    {
        bool ArmyTriggers(Army army);
    }

    [Serializable]
    public class TriggerConditions : ITriggerCondition
    {
        [SerializeField]
        private TriggerCondition[] _conditions;

        public bool ArmyTriggers(Army army)
        {
            if (_conditions.Length == 0)
            {
                // No conditions
                return true;
            }

            return _conditions.All(a => a.ArmyTriggers(army));
        }
    }

    [Serializable]
    public class TriggerCondition : ITriggerCondition
    {
        public enum Type
        {
            AnyArmy,
            ArmyContainsUnit,
        }

        [SerializeField]
        private Type _type;

        private bool ShowSpecificUnit() => _type == Type.ArmyContainsUnit;

        [ShowIf("ShowSpecificUnit")]
        public UnitData SpecificUnit;

        public bool ArmyTriggers(Army army)
        {
            if (_type == Type.AnyArmy)
            {
                return true;
            }

            if (_type == Type.ArmyContainsUnit)
            {
                if (army.HasUnitType(SpecificUnit))
                {
                    return true;
                }
            }

            return false;
        }

    }
}