using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IEquatable<Unit>
{
    public FormationPair Formation;

    public UnitData Data;

    public UnitInfo Info;

    public bool IsInFormation;

    public string ID { get; private set; }

    public Unit()
    {
        ID = Guid.NewGuid().ToString();
    }

    public bool IsDead()
    {
        return this.Info.CurrentStats.HP <= 0;
    }

    public ICombatStrategy GetStrategy()
    {
        // TODO: need some arg to decide which to pick
        return Data.DefaultStrategy;
    }

    public bool Equals(Unit other)
    {
        return this == other || this.ID == other.ID;
    }
}
