using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitStatType
{
    HP,
    Power,
    Defense,
    Speed,
    Level,
    StartingBlockShields,
    Command,
    ActiveMoraleShields,
    ActiveBlockShields,
}

public class UnitStatChangeEventArgs : EventArgs
{
    public UnitStatType Type { get; }
    public UnitStats Current { get; }

    public UnitStatChangeEventArgs(UnitStatType type, UnitStats current)
    {
        Type = type;
        Current = current;
    }
}

[Serializable]
public class UnitStats
{
    [SerializeField]
    private int _hp;

    [SerializeField]
    private int _power;

    [SerializeField]
    private int _defense;

    [SerializeField]
    private int _speed;

    [SerializeField]
    private int _level = 1;

    /// <summary>
    /// How many Block Shields the unit starts combat with.
    /// </summary>
    [Tooltip("How many Block Shields the unit starts combat with. For buffs, this is a onetime bonus to block shields.")]
    [SerializeField]
    private int _startingBlockShields = 0;

    /// <summary>
    /// For officers, it's how many commands they can issue.
    /// </summary>
    [SerializeField]
    private int _command = 0;

    /// <summary>
    /// Shields available in combat start due to high morale
    /// </summary>
    private int _moraleShields = 0;


    /// <summary>
    /// Shields available in combat start due to defensive properties
    /// </summary>
    private int _blockShields = 0;

    public int HP { get => _hp; set { _hp = value; FireStatChange(UnitStatType.HP); } }
    public int Power { get => _power; set { _power = value; FireStatChange(UnitStatType.Power); } }  
    public int Defense { get => _defense; set { _defense = value; FireStatChange(UnitStatType.Defense); } }  
    public int Speed { get => _speed; set { _speed = value; FireStatChange(UnitStatType.Speed); } }  
    public int Level { get => _level; set { _level = value; FireStatChange(UnitStatType.Level); } }  
    public int StartingBlockShields { get => _startingBlockShields; set { _startingBlockShields = value; FireStatChange(UnitStatType.StartingBlockShields); } }  
    public int Commands { get => _command; set { _command = value; FireStatChange(UnitStatType.Command); } }  
    public int ActiveMoraleShields { get => _moraleShields; set { _moraleShields = value; FireStatChange(UnitStatType.ActiveMoraleShields); } }  
    public int ActiveBlockShields { get => _blockShields; set { _blockShields = value; FireStatChange(UnitStatType.ActiveBlockShields); } }  

    public event EventHandler<UnitStatChangeEventArgs> StatChanged;

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

            ActiveMoraleShields = this.ActiveMoraleShields + other.ActiveMoraleShields,
            ActiveBlockShields = this.ActiveBlockShields + other.ActiveBlockShields,
            StartingBlockShields = this.StartingBlockShields + other.StartingBlockShields,

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


            // not affected
            Level = this.Level,
            ActiveMoraleShields = this.ActiveMoraleShields,
            ActiveBlockShields = this.ActiveBlockShields,
            StartingBlockShields = this.StartingBlockShields,
        };
    }

    private void FireStatChange(UnitStatType type)
    {
        StatChanged?.Invoke(this, new UnitStatChangeEventArgs(type, this));
    }
}
