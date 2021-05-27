using FlavBattle.Entities;
using FlavBattle.Entities.Data;
using FlavBattle.Entities.Modifiers;
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

    public List<PerkData> Perks = new List<PerkData>();

    public ModifierSet ModifierSet { get; } = new ModifierSet();

    public bool IsOfficer { get; private set; } = false;

    public string Name { get; private set; }

    public event EventHandler<UnitStatChangeEventArgs> StatChanged;

    public UnitInfo(UnitData data, Faction faction, int level = 1, bool isOfficer = false)
    {
        this.Data = data;
        this.MaxStats = data.RollStartingStats(level);
        this.CurrentStats = this.MaxStats.Clone();
        this.Name = data.RollName();
        this.Portrait = data.RollPortrait();
        this.Faction = faction;

        for (var i = 1; i <= level; i++)
        {
            var perk = Data.RollPerk(i);
            if (perk != null)
            {
                Perks.Add(perk);
                ModifierSet.AddModifier(new PerkModifier(perk));
            }
        }

        // TODO: based on race or class or stuff
        ModifierSet.AddModifier(new DefaultModifier());

        this.Actions.AddRange(data.StartingActions);
        this.IsOfficer = isOfficer;
        
        if (isOfficer)
        {
            // TODO: different command amounts
            this.CurrentStats.Commands = 3;

            OfficerAbilities.AddRange(data.DefaultOfficerAbilities);

            // TODO: add more abilities
            var ability = data.RollNewOfficerAbility(level, OfficerAbilities);
            if (ability != null)
            {
                OfficerAbilities.Add(ability);
            }
        }

        this.CurrentStats.StatChanged += (obj, e) => StatChanged?.Invoke(obj, e);
    }
}
