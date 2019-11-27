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

    public UnitInfo(UnitData data)
    {
        this.Data = data;
        this.CurrentStats = data.RollStats();
    }
}
