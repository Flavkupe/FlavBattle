using FlavBattle.Combat.Event;
using FlavBattle.Tilemap;
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

    bool IsDestroyed { get; }
}

/// <summary>
/// Interface for an IArmy type that can go into combat and do
/// combat stuff.
/// </summary>
public interface ICombatArmy : IArmy
{
    IEnumerable<CombatConditionalEvent> CombatEvents { get; }
}

public static class ArmyExtensions
{
    public static List<Unit> GetUnits(this IArmy army, bool liveOnly = false)
    {
        if (army?.Formation == null )
        {
            return new List<Unit>();
        }

        return army.Formation.GetUnits(liveOnly);
    }
}