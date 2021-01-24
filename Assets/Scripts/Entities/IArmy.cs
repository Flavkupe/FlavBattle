using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmy : IOwnedEntity
{
    string ID { get; }

    Formation Formation { get; }

    FightingStance Stance { get; set; }

    Morale Morale { get; }

    TileInfo CurrentTileInfo { get; }
}

public static class ArmyExtensions
{
    public static List<Unit> GetUnits(this IArmy army, bool liveOnly = false)
    {
        return army.Formation.GetUnits(liveOnly);
    }
}