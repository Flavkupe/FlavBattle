﻿using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyMapSpawn : MonoBehaviour
{
    [AssetIcon]
    public Sprite Icon => Formation?.Icon;

    [Required]
    public Army ArmyTemplate;

    public FactionData Faction;

    [Tooltip("If true, will create a new instance from ArmyTemplate. If false, it will instead enable it. Note that if this is false, only one army can be spawned.")]
    public bool CreateNewInstance = true;

    [Tooltip("Whether the army is spawned at the map start. If false, SpawnArmy must be triggered.")]
    public bool SpawnOnStart = true;

    [Tooltip("How much morale this army spawns with")]
    [SerializeField]
    private int _startingMorale = 100;

    [Required]
    public FormationData Formation;

    public event EventHandler<ArmyMapSpawn> SpawnTriggered;

    private bool _spawned = false;

    /// <summary>
    /// Gets the last spawned army object from this spawner. Returns
    /// null if nothing has been spawned.
    /// </summary>
    public Army SpawnedArmy { get; private set; } 

    /// <summary>
    /// Triggers a spawn event based on this spawner's settings
    /// </summary>
    public void TriggerSpawn()
    {
        SpawnTriggered?.Invoke(this, this);
    }

    public Army SpawnArmy()
    {
        if (_spawned && !CreateNewInstance)
        {
            Debug.LogWarning("Attempting to spawn multiple armies with CreateNewInstance set to false!");
            return null;
        }

        _spawned = true;

        var army = CreateNewInstance ? Instantiate(ArmyTemplate) : ArmyTemplate;
        var formation = Formation.CreateFormation(Faction.Faction, army);
        army.SetFormation(formation);

        // Unparent so that it doesn't disappear after spawner is destroyed
        army.transform.SetParent(null);

        // Enable in case it's not enabled
        army.Show();
        
        army.SetFaction(Faction);
        army.transform.position = this.transform.position;
        army.Morale.Current = _startingMorale;

        var unit = army.Formation?.GetOfficer();
        if (unit == null)
        {
            unit = army.Formation.GetUnits(false).GetRandom();
        }

        var name = unit?.UnitName ?? "Empty";
        army.name = $"Army [{name}]";

        SpawnedArmy = army;
        return army;
    }
}
