using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitStats
{
    public int HP;

    public int Power;

    public int Defense;

    public int Speed;

    public int Level = 1;

    public UnitStats Clone()
    {
        return this.MemberwiseClone() as UnitStats;
    }

    public UnitStats Combine(UnitStats other)
    {
        return new UnitStats()
        {
            HP = this.HP + other.HP,
            Power = this.Power + other.Power,
            Defense = this.Defense + other.Defense,
            Speed = this.Speed + other.Speed,
            Level = Math.Max(Level, other.Level),
        };
    }
}
