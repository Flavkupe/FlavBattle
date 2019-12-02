using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapEventType
{
    MapPaused,
    MapUnpaused,
}

public class GameEventManager : MonoBehaviour
{
    public event EventHandler<MapEventType> MapEvent;

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
}
