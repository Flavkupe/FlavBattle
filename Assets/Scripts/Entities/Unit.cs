using FlavBattle.Entities;
using FlavBattle.Entities.Data;
using FlavBattle.Trace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IEquatable<Unit>, IHasTraceData
{
    public FormationPair Formation;

    public UnitData Data;

    public UnitInfo Info;

    /// <summary>
    /// Current army unit is in. Can be null if unit is not in an army.
    /// </summary>
    public IArmy CurrentArmy { get; set; }

    public bool IsInFormation;

    public string ID { get; private set; }

    public bool IsOfficer => Info.IsOfficer;

    /// <summary>
    /// Display name of the unit.
    /// </summary>
    public string UnitName => Info.Name;

    public Unit()
    {
        ID = Guid.NewGuid().ToString();
    }

    public bool IsDead()
    {
        return this.Info.CurrentStats.HP <= 0;
    }

    /// <summary>
    /// Returns true if the unit is exactly the same as the
    /// one passed in (matches specific unit ID)
    /// </summary>
    public bool Equals(Unit other)
    {
        return this == other || this.ID == other.ID;
    }

    /// <summary>
    /// Returns true if the unit is the same type as the other
    /// unit. For example, exact same class unit ID. That is,
    /// same special type or base type if not a special unit.
    /// </summary>
    public bool SameType(UnitData other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Data.UnitID == other.UnitID;
    }

    public UnitStatSummary GetStatSummary()
    {
        var summary = new UnitStatSummary();
        this.Info.ModifierSet.ApplyToStatSummary(summary, this);
        return summary;
    }

    public TraceData GetTrace()
    {
        var trace = TraceData.ChildTrace($"Unit [{this.UnitName}]");
        trace.Key = this.ID;
        return trace;
    }
}
