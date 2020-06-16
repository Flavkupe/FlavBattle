using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitGenerator
{
    public static Unit MakeUnit(Faction faction, int level = 1, bool isOfficer = false)
    {
        return MakeUnit(ResourceHelper.Units.GetRandom(), faction, level, isOfficer);
    }

    public static Unit MakeUnit(UnitData data, Faction faction, int level = 1, bool isOfficer = false)
    {
        if (data == null)
        {
            return null;
        }

        var unit = new Unit()
        {
            Data = data,
            Info = new UnitInfo(data, faction, level, isOfficer)
        };

        return unit;
    }
}
