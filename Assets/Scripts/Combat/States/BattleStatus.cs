using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        CombatPhase,
    }

    public bool TurnExecuting { get; set; } = false;

    public BattleStage Stage { get; set; } = BattleStage.NotInCombat;
    public IArmy PlayerArmy { get; set; }
    public IArmy OtherArmy { get; set; }
    public IArmy FleeingArmy { get; set; }    
    public int Round { get; set; } = 0;
    public List<Combatant> Combatants { get; } = new List<Combatant>();
    public Queue<Combatant> TurnQueue { get; } = new Queue<Combatant>();
    public Queue<OfficerAbilityData> AbilityQueue { get; } = new Queue<OfficerAbilityData>();

    public GameEventManager GameEventManager { get; }
    public BattleDisplay BattleDisplay { get; }
    public BattleUIPanel BattleUIPanel { get; }

    public BattleStatus(GameEventManager gem, BattleDisplay bd, BattleUIPanel buip)
    {
        this.GameEventManager = gem;
        this.BattleDisplay = bd;
        this.BattleUIPanel = buip;
    }

    public void Init(IArmy playerArmy, IArmy otherArmy)
    {
        this.PlayerArmy = playerArmy;
        this.OtherArmy = otherArmy;
        this.Stage = BattleStage.NotInCombat;
        this.FleeingArmy = null;
        this.Combatants.Clear();
        this.TurnQueue.Clear();
        this.AbilityQueue.Clear();
        this.Round = 0;
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
        combatant.CombatFormationSlot.ClearContents();
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
    /// Checks if any army is fleeing and sets state accordingly.
    /// If so, returns the army and sets fleeing state. If not, returns null.
    /// </summary>
    /// <returns></returns>
    public IArmy CheckForFleeingArmy()
    {
        if (PlayerArmy.Morale.Current - OtherArmy.Morale.Current > 20.0f)
        {
            // Enemy flees due to morale diff
            return OtherArmy;
        }
        else if (OtherArmy.Morale.Current - PlayerArmy.Morale.Current > 20.0f)
        {
            // Player flees due to morale difference
            return PlayerArmy;
        }

        // Nobody is fleeing
        return null;
    }
}