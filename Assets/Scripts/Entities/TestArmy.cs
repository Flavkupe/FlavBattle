using FlavBattle.Tilemap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestArmy : IArmy
{
    public string ID { get; } = Guid.NewGuid().ToString();

    public Formation Formation { get; set; } = new Formation();

    public FactionData Faction { get; set; }

    public FightingStance Stance { get; set; }

    public Morale Morale { get; } = new Morale();

    public TileInfo CurrentTileInfo { get; set; }
}
