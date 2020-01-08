﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapEventType
{
    MapPaused,
    MapUnpaused,
}

public class CombatEndedEventArgs
{
    public Army Winner { get; set; }
    public Army Loser { get; set; }
}

public class CombatStartedEventArgs
{
    public Army Player { get; set; }
    public Army Enemy { get; set; }
}

/// <summary>
/// Helper for state shortcuts, such as checking if
/// game is paused
/// </summary>
public static class GameState
{
    public static bool IsMapPaused => GameEventManager.IsMapPaused;
}

public class GameEventManager : MonoBehaviour
{
    public bool DebugTrace = false;

    public event EventHandler<MapEventType> MapEvent;

    public event EventHandler<CombatEndedEventArgs> CombatEndedEvent;

    public event EventHandler<CombatStartedEventArgs> CombatStartedEvent;

    public event EventHandler<Unit> UnitDeployed;

    public event EventHandler<Unit> UnitGarrisoned;

    /// <summary>
    /// Whether or not the map is paused (for example, Armies should not move)
    /// </summary>
    public static bool IsMapPaused { get; private set; }

    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
        Utils.TraceEnabled = DebugTrace;
    }

    public void TriggerMapEvent(MapEventType mapEvent)
    {
        if (mapEvent == MapEventType.MapPaused)
        {
            IsMapPaused = true;
        }
        else if (mapEvent == MapEventType.MapUnpaused)
        {
            IsMapPaused = false;
        }

        MapEvent?.Invoke(this, mapEvent);
    }

    public void TriggerUnitDeployed(Unit unit)
    {
        UnitDeployed?.Invoke(this, unit);
    }

    public void TriggerCombatStartedEvent(Army player, Army enemy)
    {
        CombatStartedEvent?.Invoke(this, new CombatStartedEventArgs
        {
            Player = player,
            Enemy = enemy
        });
    }

    public void TriggerCombatEndedEvent(Army winner, Army loser)
    {
        CombatEndedEvent?.Invoke(this, new CombatEndedEventArgs
        {
            Winner = winner,
            Loser = loser,
        });
    }
    public void TriggerUnitGarrisoned(Unit e)
    {
        UnitGarrisoned?.Invoke(this, e);
    }
}
