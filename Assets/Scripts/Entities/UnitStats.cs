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

    public UnitStats Clone()
    {
        return this.MemberwiseClone() as UnitStats;
    }
}
