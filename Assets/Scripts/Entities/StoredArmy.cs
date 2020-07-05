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

    public Morale Morale { get; } = new Morale();

    public StoredArmy(Army army)
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
        Formation = new Formation();
        Faction = faction;
    }
}
