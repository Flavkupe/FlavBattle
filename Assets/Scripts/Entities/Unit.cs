using FlavBattle.Entities;
using FlavBattle.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IEquatable<Unit>
{
    public FormationPair Formation;

    public UnitData Data;

    public UnitInfo Info;

    /// <summary>
    /// The latest summary of stats calculated for the unit. Used to show
    /// summary of certain stats for the UI, such as in combat.
    /// </summary>
    public UnitStatSummary StatSummary { get; } = new UnitStatSummary();

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
        return this.Data.UnitID == other.UnitID;
    }
}
