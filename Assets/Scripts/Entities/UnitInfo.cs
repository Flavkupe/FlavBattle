using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitInfo
{
    public UnitData Data { get; private set; }
    public UnitStats CurrentStats { get; private set; }

    public UnitInfo()
    {
    }

    public UnitInfo(UnitData data, int level = 1)
    {
        this.Data = data;
        this.CurrentStats = data.RollStats(level);
    }
}
