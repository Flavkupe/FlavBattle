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

    public bool Equals(Unit other)
    {
        return this == other || this.ID == other.ID;
    }
}
