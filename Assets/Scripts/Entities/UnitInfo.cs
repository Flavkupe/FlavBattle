using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitInfo
{
    public UnitData Data { get; set; }
    public UnitStats CurrentStats { get; }

    public UnitInfo()
    {
    }

    public UnitInfo(UnitData data)
    {
        this.Data = data;
        this.CurrentStats = data.BaseStats.Clone();
    }
}
