using FlavBattle.Combat;
using FlavBattle.Entities;
using FlavBattle.Entities.Modifiers;
using System.Linq;
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

    public ICombatArmy Enemies;
    public ICombatArmy Allies;

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
    public UnitStats GetCombatCombinedStats() 
    {
        var summary = GetUnitStatSummary();
        var modifierStats = summary.GetAccumulatedStats();
        return Unit.Info.CurrentStats.GetCombined(modifierStats);
    }

    public int UnitMoraleBonus => Unit.Info.Morale.GetDefaultBonus();
    public Morale UnitMorale => Unit.Info.Morale;
    public int ArmyMorale => Allies.Morale.Current;
    public bool IsInPlayerArmy => Allies.Faction.IsPlayerFaction;

    /// <summary>
    /// Gets the unit stats, combined with army stats as well.
    /// </summary>
    /// <returns></returns>
    public UnitStatSummary GetUnitStatSummary()
    {
        return this.Unit.GetStatSummary(true);
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
    }

    /// <summary>
    /// Ticks the combatant for a new turn (such as for buffs)
    /// </summary>
    public void ProcessTurnStart()
    {
        this.Unit.Info.ModifierSet.Tick(ModifierTickType.CombatTurnStart);
    }

    public void ProcessStanceChanged()
    {
    }

    public void ProcessCombatStart()
    {
        var combinedStats = this.GetCombatCombinedStats();

        // Cap starting morale to army morale
        Unit.Info.Morale.Current = Math.Min(Unit.Info.Morale.Current, Allies.Morale.Current);

        this.Unit.Info.ModifierSet.Tick(ModifierTickType.CombatStart);

        // like HP, this goes on CurrentStats
        var startingShields = combinedStats.StartingBlockShields;
        Unit.Info.CurrentStats.ActiveBlockShields = startingShields;

        // TODO: can we do this some other way?
        AddBlockShieldIcons(startingShields);

        if (CombatUnit != null)
        {
            // Update morale etc to match new values
            CombatUnit.UpdateUIComponents();
        }
    }

    public void ProcessCombatEnd()
    {
        this.Unit.Info.ModifierSet.Tick(ModifierTickType.CombatEnd);
        Unit.Info.CurrentStats.ActiveBlockShields = 0;
    }

    public void TakeDamage(int damage)
    {
        var info = Unit.Info;
        info.CurrentStats.HP -= damage;
    }

    public void TakeMoraleDamage(int moraleDamage)
    {
        var info = Unit.Info;
        this.Unit.Info.Morale.ChangeMorale(-moraleDamage);
    }

    /// <summary>
    /// Adds a number of block shields and adds the buff icons for them.
    /// </summary>
    private void AddBlockShieldIcons(int numBlockShields)
    {
        if (CombatUnit == null)
        {
            return;
        }

        for (var i = 0; i < numBlockShields; i++)
        {
            this.CombatUnit.AddBuffIcon(CombatBuffIcon.BuffType.BlockShield);
        }
    }
}
