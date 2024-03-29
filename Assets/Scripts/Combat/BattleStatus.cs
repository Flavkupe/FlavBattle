﻿using FlavBattle.Combat.Event;
using FlavBattle.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BattleStatus
{
    public enum Winner
    {
        None,
        Left,
        Right,
    }


    public enum BattleStage
    {
        NotInCombat,

        InitCombat,

        PreCombatStart,

        SelectStance,

        CombatConditionalEvents,

        DetermineTurnOrder,

        CombatPhase,
    }

    public bool TurnExecuting { get; set; } = false;

    public BattleStage Stage { get; set; } = BattleStage.NotInCombat;
    public ICombatArmy PlayerArmy { get; set; }
    public ICombatArmy OtherArmy { get; set; }
    public ICombatArmy FleeingArmy { get; set; }    
    public int Round { get; set; } = 0;
    public List<Combatant> Combatants { get; } = new List<Combatant>();
    public Queue<Combatant> TurnQueue { get; } = new Queue<Combatant>();
    public Queue<OfficerAbilityData> AbilityQueue { get; } = new Queue<OfficerAbilityData>();

    public List<CombatConditionalEvent> ConditionalEvents { get; } = new List<CombatConditionalEvent>();

    public GameEventManager GameEventManager { get; }
    public BattleDisplay BattleDisplay { get; }
    public BattleUIPanel BattleUIPanel { get; }

    /// <summary>
    /// Whether or not the player's stance is locked. If
    /// false, it will prompt for a stance change on the next turn.
    /// </summary>
    public bool IsStanceLocked { get; set; }

    public BattleStatus(GameEventManager gem, BattleDisplay bd, BattleUIPanel buip)
    {
        this.GameEventManager = gem;
        this.BattleDisplay = bd;
        this.BattleUIPanel = buip;
    }

    public void Init(ICombatArmy playerArmy, ICombatArmy otherArmy)
    {
        this.PlayerArmy = playerArmy;
        this.OtherArmy = otherArmy;
        this.Stage = BattleStage.NotInCombat;
        this.FleeingArmy = null;
        this.Combatants.Clear();
        this.TurnQueue.Clear();
        this.AbilityQueue.Clear();
        this.Round = 0;

        this.ConditionalEvents.Clear();
        this.ConditionalEvents.AddRange(playerArmy.CombatEvents);
        this.ConditionalEvents.AddRange(otherArmy.CombatEvents);
        foreach (var combatEvent in ConditionalEvents.Select(a => a.Event).OfType<ICombatGameEvent>())
        {
            combatEvent.SetBattleContext(this);
        }
    }

    public Combatant GetPlayerOfficer()
    {
        var combatants = GetCombatants(PlayerArmy.Formation.GetUnits());
        var officer = combatants.First(a => a.Unit.IsOfficer);
        return officer;
    }

    public List<Combatant> GetCombatants(IArmy army)
    {
        return GetCombatants(army.Formation.GetUnits());
    }

    public List<Combatant> GetCombatants(List<Unit> units)
    {
        return Combatants.Where(a => units.Any(b => a.Unit.ID == b.ID)).ToList();
    }

    public IEnumerable<Combatant> FindTargets(Combatant combatant)
    {
        var enemies = Combatants.Where(a => a.Left != combatant.Left);

        // Single attack
        var enemy = enemies.ToList().GetRandom();
        return enemy == null ? new List<Combatant>() : new List<Combatant>() { enemy };
    }

    public Winner CheckWinner()
    {
        if (!Combatants.Any(a => a.Left))
        {
            return Winner.Right;
        }
        else if (!Combatants.Any(a => !a.Left))
        {
            return Winner.Left;
        }

        return Winner.None;
    }

    public Combatant PeekNextLiveCombatant()
    {
        while (TurnQueue.Count > 0)
        {
            var current = TurnQueue.Peek();
            if (!current.Unit.IsDead())
            {
                return current;
            }
            else
            {
                // remove dead combatants from queue
                TurnQueue.Dequeue();
            }
        }

        return null;
    }

    public Combatant GetNextCombatant()
    {
        while (TurnQueue.Count > 0)
        {
            var current = TurnQueue.Dequeue();
            if (!current.Unit.IsDead())
            {
                return current;
            }
        }

        return null;
    }

    public void ClearCombatant(Combatant combatant)
    {
        // combatant.CombatFormationSlot.ClearContents();
        Combatants.Remove(combatant);
    }

    /// <summary>
    /// Gets the army that is the oponent of army in this fight.
    /// </summary>
    public IArmy GetOpponent(IArmy army)
    {
        if (army == PlayerArmy)
        {
            return OtherArmy;
        }

        return PlayerArmy;
    }

    /// <summary>
    /// Checks if any army should be fleeing.
    /// If so, returns the army. If not, returns null.
    /// </summary>
    public ICombatArmy CheckForFleeingArmy(int currentBout)
    {
        // TODO: incorporate Leadership stat and bravery
        var playerMorale = PlayerArmy.Morale.Current;
        var otherMorale = OtherArmy.Morale.Current;
        var playerShouldFlee = ArmyShouldFlee(PlayerArmy, currentBout);
        var otherShouldFlee = ArmyShouldFlee(OtherArmy, currentBout);

        if (otherShouldFlee && playerShouldFlee)
        {
            // If both want to flee, the one with less morale flees
            if (otherMorale < playerMorale)
            {
                // Enemy flees due to morale diff
                return OtherArmy;
            }
            else if (otherMorale > playerMorale)
            {
                return PlayerArmy;
            }

            // On equal morale, both stay
        }
        else if (otherShouldFlee)
        {
            return OtherArmy;
        }
        else if (playerShouldFlee)
        {
            return PlayerArmy;
        }

        // nobody flees
        return null;
    }

    private bool ArmyShouldFlee(IArmy army, int currentBout)
    {
        var moraleThreshold = GRM.Instance?.FleeingArmyThreshold ?? 75;
        if (army.Morale.Current > moraleThreshold)
        {
            return false;
        }

        var units = army.GetUnits(true);
        var averageBout = units.Average(a => (float)a.Data.BoutsToFlee);
        var boutToFlee = Mathf.RoundToInt(averageBout);
        if (boutToFlee > currentBout)
        {
            return false;
        }

        return true;
    }
}

