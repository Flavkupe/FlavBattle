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

    public List<CombatAction> Actions { get; } = new List<CombatAction>();

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

        this.Actions.AddRange(data.StartingActions);
        this.IsOfficer = isOfficer;
        
        if (isOfficer)
        {
            // TODO: different command amounts
            this.CurrentStats.Command = 3;

            OfficerAbilities.AddRange(data.DefaultOfficerAbilities);

            // TODO: add more abilities
            var ability = data.RollNewOfficerAbility(level, OfficerAbilities);
            if (ability != null)
            {
                OfficerAbilities.Add(ability);
            }
        }
    }
}
