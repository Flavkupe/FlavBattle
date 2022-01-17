using FlavBattle.Entities.Modifiers;
using FlavBattle.Tilemap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredArmy : IArmy
{
    public string ID { get; private set; }

    public Formation Formation { get; private set; }

    public FactionData Faction { get; private set; }

    public FightingStance Stance { get; set; }

    public GridTile CurrentTileInfo => null;

    public Morale Morale { get; } = new Morale();

    public bool IsDestroyed
    {
        get
        {
            return Formation == null || Formation.GetUnits().TrueForAll(a => a.IsDead());
        }
    }

    public StoredArmy(IArmy army)
    {
        ID = army.ID;
        Formation = army.Formation;
        Faction = army.Faction;
    }

    public StoredArmy(string id, Formation formation, FactionData faction)
    {
        ID = id;
        Formation = formation;
        Faction = faction;
    }

    public StoredArmy(FactionData faction)
    {
        ID = Guid.NewGuid().ToString();
        Formation = new Formation(this);
        Faction = faction;
    }

    public StoredArmy(FactionData faction, FormationData data)
    {
        ID = Guid.NewGuid().ToString();
        Formation = data.CreateFormation(faction.Faction, this);
        Faction = faction;
    }

    public ModifierSet GetModifiers()
    {
        return new UnitModifierSet();
    }
}
