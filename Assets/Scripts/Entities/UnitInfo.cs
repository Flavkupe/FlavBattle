using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitInfo
{
    public UnitData Data { get; private set; }
    public UnitStats CurrentStats { get; private set; }
    public UnitStats MaxStats { get; private set; }

    public Faction Faction { get; private set; }

    public Sprite Portrait { get; private set; }

    public List<CombatAbilityData> Abilities { get; } = new List<CombatAbilityData>();

    public string Name { get; private set; }

    public UnitInfo()
    {
    }

    public UnitInfo(UnitData data, Faction faction, int level = 1)
    {
        this.Data = data;
        this.MaxStats = data.RollStats(level);
        this.CurrentStats = this.MaxStats.Clone();
        this.Name = data.RollName();
        this.Portrait = data.RollPortrait();
        this.Faction = faction;
        this.Abilities.AddRange(data.StartingAbilities);
    }

    public void LearnAbility(CombatAbilityData data)
    {
        if (!Abilities.Contains(data))
        {
            Abilities.Add(data);
        }
    }
}
