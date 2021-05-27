using FlavBattle.Combat;
using FlavBattle.Entities;
using FlavBattle.Entities.Modifiers;
using System;

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
    /// Visual stuff for this unit
    /// </summary>
    public CombatUnit CombatUnit => CombatFormationSlot?.CurrentUnit;

    /// <summary>
    /// Stat changes due to buffs and temporary combat effects
    /// </summary>
    private UnitStats _modifierStats = new UnitStats();

    /// <summary>
    /// Latest stat summary for unit.
    /// </summary>
    public UnitStatSummary StatSummary { get; private set; } = new UnitStatSummary();

    /// <summary>
    /// Use this to get the current unit stats combined with stat changes.
    /// Changing this value will not change current stats! For that, use
    /// Unit.Info.CurrentStats for permanent or ApplyStatChanges for only
    /// within combat.
    /// </summary>
    public UnitStats CombatCombinedStats => Unit.Info.CurrentStats.GetCombined(_modifierStats);

    public int UnitMoraleBonus => Unit.Info.Morale.GetDefaultBonus();
    public Morale UnitMorale => Unit.Info.Morale;
    public int ArmyMorale => Allies.Morale.Current;
    public bool IsInPlayerArmy => Allies.Faction.IsPlayerFaction;

    /// <summary>
    /// Change Stats for the Combatant temporarily, just for the combat duration.
    /// </summary>
    public void RefreshStatChanges()
    {
        StatSummary = this.Unit.GetStatSummary();
        var stats = StatSummary.GetAccumulatedStats();
        _modifierStats = stats;
    }

    /// <summary>
    /// Applies a buff for this unit. If duration is 0, it lasts for entire
    /// combat. Otherwise it lasts that many turns (ticks at turn start).
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="duration"></param>
    public void AddStatBuff(string name, UnitStats buff, int duration = 0)
    {
        var type = duration == 0 ? CombatEffectModifierDuration.AllCombat : CombatEffectModifierDuration.Turns;
        var modifier = new CombatEffectModifier(name, buff, type, duration);
        this.Unit.Info.ModifierSet.AddModifier(modifier);

        // TODO add block shields for buff
        RefreshStatChanges();
    }

    /// <summary>
    /// Ticks the combatant for a new turn (such as for buffs)
    /// </summary>
    public void ProcessTurnStart()
    {
        this.Unit.Info.ModifierSet.TickModifiers(ModifierTickType.CombatTurnStart);
        RefreshStatChanges();
    }

    public void ProcessStanceChanged()
    {
        RefreshStatChanges();
    }

    public void ProcessCombatStart()
    {
        this.Unit.Info.ModifierSet.TickModifiers(ModifierTickType.CombatStart);
        RefreshStatChanges();

        // like HP, this goes on CurrentStats
        var startingShields = _modifierStats.StartingBlockShields + Unit.Info.CurrentStats.StartingBlockShields;
        Unit.Info.CurrentStats.ActiveBlockShields = startingShields;

        // TODO: can we do this some other way?
        AddBlockShieldIcons(startingShields);
    }

    public void ProcessCombatEnd()
    {
        this.Unit.Info.ModifierSet.TickModifiers(ModifierTickType.CombatEnd);
        Unit.Info.CurrentStats.ActiveBlockShields = 0;
    }

    /// <summary>
    /// Adds a number of block shields and adds the buff icons for them.
    /// </summary>
    private void AddBlockShieldIcons(int numBlockShields)
    {
        for (var i = 0; i < numBlockShields; i++)
        {
            this.CombatUnit.AddBuffIcon(CombatBuffIcon.BuffType.BlockShield);
        }
    }
}