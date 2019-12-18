using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction
{
    Rebels = 1,
    KingsMen = 2,
}

[CreateAssetMenu(fileName = "Faction Data", menuName = "Custom/Factions/Faction Data", order = 1)]
public class FactionData : ScriptableObject
{
    public bool IsPlayerFaction;

    public Faction Faction;

    [AssetIcon]
    public Sprite Flag;

    public string Name;
}
