﻿using FlavBattle.Combat;
using FlavBattle.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Combatant
{
    public Combatant(CombatFormationSlot slot)
    {
        CombatFormationSlot = slot;
        slot.CurrentUnit.RightClicked += HandleCombatantRightClicked;
    }

    private void HandleCombatantRightClicked(object sender, EventArgs e)
    {
        RightClicked?.Invoke(this, this);
    }

    private List<StatsBuff> _statBuffs = new List<StatsBuff>();

    /// <summary>
    /// Fires when the representation of this unit is right-clicked
    /// </summary>
    public event EventHandler<Combatant> RightClicked;

    public bool Left;
    public Unit Unit;
    public FormationColumn Col;
    public FormationRow Row;
    public CombatFormation CombatFormation;
    public CombatFormationSlot CombatFormationSlot { get; private set; }

    public IArmy Enemies;
    public IArmy Allies;

    /// <summary>
    /// Stat changes due to buffs and temporary combat effects
    /// </summary>
    public UnitStats StatChanges { get; private set; } = new UnitStats
    {
        // Level not affected by stat changes
        Level = 0
    };

    /// <summary>
    /// Visual stuff for this unit
    /// </summary>
    public CombatUnit CombatUnit => CombatFormationSlot?.CurrentUnit;

    /// <summary>
    /// Use this to get the current unit stats combined with stat changes.
    /// Changing this value will not change current stats! For that, use
    /// Unit.Info.CurrentStats for permanent or ApplyStatChanges for only
    /// within combat.
    /// </summary>
    public UnitStats CombatCombinedStats => Unit.Info.CurrentStats.GetCombined(StatChanges);

    public int UnitMoraleBonus => Unit.Info.Morale.GetDefaultBonus();
    public Morale UnitMorale => Unit.Info.Morale;
    public int ArmyMorale => Allies.Morale.Current;
    public bool IsInPlayerArmy => Allies.Faction.IsPlayerFaction;

    /// <summary>
    /// Change Stats for the Combatant temporarily, just for the combat duration.
    /// </summary>
    private void ApplyStatChanges(UnitStats changes)
    {
        StatChanges.Combine(changes);
    }

    /// <summary>
    /// Applies a buff for this unit. If duration is 0, it lasts for entire
    /// combat. Otherwise it lasts that many turns (ticks at turn start).
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="duration"></param>
    public void AddStatBuff(UnitStats buff, int duration = 0)
    {
        ApplyStatChanges(buff);
        if (duration > 0)
        {
            var statBuff = new StatsBuff(this, buff, duration);
            _statBuffs.Add(statBuff);
        }

        // Add block shields for this buff
        AddBlockShields(buff.StartingBlockShields);
    }

    /// <summary>
    /// Adds a number of block shields and adds the buff icons for them.
    /// </summary>
    public void AddBlockShields(int numBlockShields)
    {
        for (var i = 0; i < numBlockShields; i++)
        {
            this.StatChanges.ActiveBlockShields++;
            this.CombatUnit.AddBuffIcon(CombatBuffIcon.BuffType.BlockShield);
        }
    }

    /// <summary>
    /// Applies perk bonuses to Stats. Should be used when combat starts
    /// to set stats for combat.
    /// 
    /// This call is NOT idempotent; calling it multiple times will stack.
    /// </summary>
    public void ApplyPerkStatBonuses()
    {
        var perkStats = StatsUtils.GetStatBonusesFromPerks(Unit, Allies);
        StatChanges.Combine(perkStats);
    }

    /// <summary>
    /// Ticks the combatant for a new turn (such as for buffs)
    /// </summary>
    public void ProcessTurnStart()
    {
        foreach (var statBuff in _statBuffs.ToList())
        {
            statBuff.TickDuration();
            if (statBuff.Expired)
            {
                _statBuffs.Remove(statBuff);
                this.ApplyStatChanges(statBuff.Buff.Multiply(-1));
            }
        }
    }

    /// <summary>
    /// This updates the latest combat attack and defense
    /// summaries. Use this to get the latest totals.
    /// </summary>
    public void UpdateAttDefStatSummaries()
    {
        this.Unit.StatSummary.Clear();
        StatsUtils.ComputeDefenseSummary(Unit, Allies, StatChanges);
        StatsUtils.ComputeAttackSummary(Unit, Allies, StatChanges);
        StatsUtils.ApplyPerkBonusesToSummary(Unit, Allies);
    }
}