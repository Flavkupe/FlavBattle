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
    public Morale Morale { get; } = new Morale();
    public Faction Faction { get; private set; }

    public Sprite Portrait { get; private set; }

    public List<CombatAbilityData> Abilities { get; } = new List<CombatAbilityData>();

    public List<OfficerAbilityData> OfficerAbilities { get; } = new List<OfficerAbilityData>();

    public bool IsOfficer { get; private set; } = false;

    public string Name { get; private set; }

    public UnitInfo()
    {
    }

    public UnitInfo(UnitData data, Faction faction, int level = 1, bool isOfficer = false)
    {
        this.Data = data;
        this.MaxStats = data.RollStats(level);
        this.CurrentStats = this.MaxStats.Clone();
        this.Name = data.RollName();
        this.Portrait = data.RollPortrait();
        this.Faction = faction;
        this.Abilities.AddRange(data.StartingAbilities);
        this.IsOfficer = isOfficer;

        if (isOfficer)
        {
            // TODO: add more abilities
            var ability = data.RollNewOfficerAbility(level, OfficerAbilities);
            if (ability != null)
            {
                OfficerAbilities.Add(ability);
            }
        }
    }

    public void LearnAbility(CombatAbilityData data)
    {
        if (!Abilities.Contains(data))
        {
            Abilities.Add(data);
        }
    }
}
