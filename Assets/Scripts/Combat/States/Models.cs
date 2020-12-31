using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IBattleState
{
    void Update(BattleStatus state);
    bool ShouldUpdate(BattleStatus state);
}

public class Combatant
{
    public bool Left;
    public Unit Unit;
    public FormationColumn Col;
    public FormationRow Row;
    public CombatFormation CombatFormation;
    public CombatFormationSlot CombatFormationSlot;

    public IArmy Enemies;
    public IArmy Allies;

    /// <summary>
    /// Stat changes due to buffs
    /// </summary>
    public UnitStats StatChanges { get; private set; } = new UnitStats
    {
        // Level not affected by stat changes
        Level = 0
    };

    public CombatUnit CombatUnit => CombatFormationSlot?.CurrentUnit;

    public void ApplyStatChanges(UnitStats changes)
    {
        StatChanges = StatChanges.Combine(changes);
    }

    public int UnitMoraleBonus => Unit.Info.Morale.GetDefaultBonus();
    public Morale UnitMorale => Unit.Info.Morale;
    public int ArmyMorale => Allies.Morale.Current;
    public bool IsInPlayerArmy => Allies.Faction.IsPlayerFaction;
}