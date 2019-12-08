using System;
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

public class GameEventManager : MonoBehaviour
{
    public event EventHandler<MapEventType> MapEvent;

    public event EventHandler<CombatEndedEventArgs> CombatEndedEvent;

    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {    
    }

    public void TriggerMapEvent(MapEventType mapEvent)
    {
        MapEvent?.Invoke(this, mapEvent);
    }

    public void TriggerCombatEndedEvent(Army winner, Army loser)
    {
        CombatEndedEvent?.Invoke(this, new CombatEndedEventArgs
        {
            Winner = winner,
            Loser = loser,
        });
    }
}
