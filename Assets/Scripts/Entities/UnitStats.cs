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

    /// <summary>
    /// For officers, it's how many commands they can issue.
    /// </summary>
    public int Command = 0;

    public UnitStats Clone()
    {
        return this.MemberwiseClone() as UnitStats;
    }

    public UnitStats Combine(params UnitStats[] others)
    {
        var combined = new UnitStats();
        foreach (var stats in others)
        {
            combined = combined.Combine(stats);
        }

        return combined;
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

    /// <summary>
    /// Gets stats affected by a multiplier
    /// </summary>
    public UnitStats Multiply(float multiplier)
    {
        return new UnitStats()
        {
            HP = (int)(this.HP * multiplier),
            Power = (int)(this.Power * multiplier),
            Defense = (int)(this.Defense * multiplier),
            Speed = (int)(this.Speed * multiplier),

            Level = this.Level, // not affected
        };
    }
}
