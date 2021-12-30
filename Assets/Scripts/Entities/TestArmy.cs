using FlavBattle.Combat.Event;
using FlavBattle.Tilemap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestArmy : ICombatArmy
{
    public TestArmy()
    {
        Formation = new Formation(this);
    }

    public string ID { get; } = Guid.NewGuid().ToString();

    public Formation Formation { get; private set; }

    public FactionData Faction { get; set; }

    public FightingStance Stance { get; set; }

    public Morale Morale { get; } = new Morale();

    public GridTile CurrentTileInfo { get; set; }

    public bool IsDestroyed => false;

    public IEnumerable<CombatConditionalEvent> CombatEvents => new List<CombatConditionalEvent>();

    public IEnumerable<IArmy> GetFlankingArmies()
    {
        return new List<IArmy>();
    }

    public IEnumerable<IArmy> GetLinkedArmies()
    {
        return new List<IArmy>();
    }
}
