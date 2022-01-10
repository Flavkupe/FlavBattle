using FlavBattle.Combat.Event;
using FlavBattle.Entities.Modifiers;
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

    GridTile CurrentTileInfo { get; }

    bool IsDestroyed { get; }

    ModifierSet GetModifiers();
}

/// <summary>
/// Interface for an IArmy type that can go into combat and do
/// combat stuff.
/// </summary>
public interface ICombatArmy : IArmy
{
    IEnumerable<CombatConditionalEvent> CombatEvents { get; }

    IEnumerable<IArmy> GetFlankingArmies();

    IEnumerable<IArmy> GetLinkedArmies();
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

    public static bool SameFaction(this IArmy army, IArmy other)
    {
        if (army == null || other == null || 
            army?.Faction?.Faction == null || other?.Faction?.Faction == null)
        {
            return false;
        }

        return army.Faction.Faction == other.Faction.Faction;
    }

    /// <summary>
    /// Gets the ratio of HP for the entire army (sum of unit
    /// current HPs divided by sum of max HPs). Value between
    /// 0.0 (no HP) to 1.0 (full HP).
    /// </summary>
    public static float GetHPPercent(this IArmy army)
    {
        var hpCurrent = 0;
        var hpTotal = 0;
        foreach (var unit in army.GetUnits())
        {
            hpCurrent += unit.Info.CurrentStats.HP;
            hpTotal += unit.Info.MaxStats.HP;
        }

        var ratio = 1.0f;
        if (hpTotal != 0)
        {
            ratio = (float)hpCurrent / (float)hpTotal;
        }

        return ratio;
    }
}
