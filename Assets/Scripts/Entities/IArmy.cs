using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmy
{
    string ID { get; }

    Formation Formation { get; }

    FactionData Faction { get; }

    FightingStance Stance { get; set; }

    Morale Morale { get; }
}
