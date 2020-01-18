using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitGenerator
{
    public static Unit MakeUnit(Faction faction, int level = 1)
    {
        return MakeUnit(ResourceHelper.Units.GetRandom(), faction, level);
    }

    public static Unit MakeUnit(UnitData data, Faction faction, int level = 1)
    {
        if (data == null)
        {
            return null;
        }

        var unit = new Unit()
        {
            Data = data,
            Info = new UnitInfo(data, faction, level)
        };

        return unit;
    }
}
