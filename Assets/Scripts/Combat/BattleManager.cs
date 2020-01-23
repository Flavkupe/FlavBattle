using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    private enum State
    {
        NotInCombat,
        AwaitingTurn,
        TurnInProgress,
        ShowWinner,
    }

    private enum Winner
    {
        None,
        Left,
        Right,
    }

    private class Combatant
    {
        public bool Left;
        public Unit Unit;
        public FormationColumn Col;
        public FormationRow Row;
        public CombatFormation CombatFormation;
        public CombatFormationSlot CombatFormationSlot;

        public IArmy Enemies;
        public IArmy Allies;
    }

    private State _state = State.NotInCombat;
    private IArmy _player;
    private IArmy _other;
    private List<Combatant> _combatants = new List<Combatant>();
    private Queue<Combatant> _turnQueue = new Queue<Combatant>();
    private GameEventManager _gameEventManager;

    public BattleDisplay BattleDisplay;

    // Start is called before the first frame update
    void Start()
    {
        _gameEventManager = FindObjectOfType<GameEventManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_state == State.NotInCombat || _state == State.TurnInProgress)
        {
            return;
        }

        if (_state == State.AwaitingTurn)
        {
            var winner = CheckWinner();
            if (winner != Winner.None)
            {
                _state = State.ShowWinner;
                StartCoroutine(ShowWinner(winner));
                return;
            }

            var current = GetNextCombatant();
            if (current != null)
            {
                _state = State.TurnInProgress;   
                StartCoroutine(DoTurn(current));
            }
            else
            {
                // Round done; rearrange turns
                ArrangeTurns();
            }
        }
    }

    public Coroutine StartCombat(IArmy player, IArmy enemy)
    {
        return StartCoroutine(StartCombatInternal(player, enemy));
    }

    private IEnumerator ShowWinner(Winner winner)
    {
        var victory = winner == Winner.Left;
        yield return BattleDisplay.ShowCombatEndSign(victory);
        yield return BattleDisplay.HideCombatScene();
        var winningArmy = victory ? _player : _other;
        var losingArmy = victory ? _other : _player;
        _gameEventManager.TriggerCombatEndedEvent(winningArmy, losingArmy);
        _state = State.NotInCombat;
    }

    private IEnumerator StartCombatInternal(IArmy player, IArmy enemy)
    {
        _gameEventManager.TriggerCombatStartedEvent(player, enemy);

        _player = player;
        _other = enemy;
        _combatants.Clear();
        _turnQueue.Clear();
        yield return BattleDisplay.InitializeCombatScene(player, enemy);
        _combatants.AddRange(GetCombatants(_player, _other, true));
        _combatants.AddRange(GetCombatants(_other, _player, false));
        _state = State.AwaitingTurn;
    }

    private IEnumerable<Combatant> GetCombatants(IArmy allies, IArmy enemies, bool left)
    {
        var combatFormation = left ? BattleDisplay.LeftFormation : BattleDisplay.RightFormation;
        return allies.Formation.GetOccupiedPositionInfo().Select(a => new Combatant
        {
            Left = left,
            Unit = a.Unit,
            Row = a.FormationPair.Row,
            Col = a.FormationPair.Col,
            CombatFormation = combatFormation,
            CombatFormationSlot = combatFormation.GetFormationSlot(a.FormationPair.Row, a.FormationPair.Col),
            Allies = allies,
            Enemies = enemies
        });
    }

    private Combatant GetNextCombatant()
    {
        while (_turnQueue.Count > 0)
        {
            var current = _turnQueue.Dequeue();
            if (!current.Unit.IsDead())
            {
                return current;
            }
        }

        return null;
    }

    private void ArrangeTurns()
    {
        foreach (var item in _combatants.OrderBy(a => a.Unit.Info.CurrentStats.Speed).Reverse())
        {
            _turnQueue.Enqueue(item);
        }
    }

    private IEnumerator DoTurn(Combatant combatant)
    {
        var stratData = combatant.Unit.GetStrategy();
        var strat = new CombatStrategy(stratData, combatant.Unit, combatant.Allies, combatant.Enemies);
        var decision = strat.Decide();

        var ability = decision.Ability;
        var targets = GetCombatants(decision.Targets);

        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name}'s turn!");

        if (targets.Count() == 0)
        {
            // TODO
            Debug.Log("No more targets!");
            _state = State.AwaitingTurn;
            yield break;
        }

        Debug.Log($"Targets: {string.Join(", ", targets.Select(a => a.Unit.Data.ClassName)) }");

        var damageRoll = combatant.Unit.Info.CurrentStats.Power;

        if (ability == null)
        {
            Debug.Log("No viable attack!");
            // yield return combatant.CombatFormationSlot.CurrentUnit.AnimateAttack();
            _state = State.AwaitingTurn;
            yield break;
        }
        
        foreach (var target in targets)
        {
            var slot = target.CombatFormationSlot;
            slot.Highlight();

            var damage = damageRoll;
            if (ability != null)
            {
                damage += ability.Damage.RandomBetween();
                yield return AnimateAbility(combatant, target, ability);
            }

            Debug.Log($"Damage roll for {damage}!");
            yield return AttackTarget(combatant, target, damage);
            slot.ResetColor();
        }

        // yield return new WaitForSeconds(0.5f);

        _state = State.AwaitingTurn;
    }

    private IEnumerator AnimateAbility(Combatant attacker, Combatant target, CombatAbilityData abilityData)
    {
        var obj = new GameObject("Ability");
        var ability = obj.AddComponent<CombatAbility>();

        ability.InitData(abilityData);
        yield return ability.StartTargetedAbility(attacker.CombatFormationSlot.CurrentUnit.gameObject, target.CombatFormationSlot.CurrentUnit.gameObject);
    }

    private IEnumerator AttackTarget(Combatant attacker, Combatant target, int damageRoll)
    {
        var slot = target.CombatFormationSlot;
        Debug.Log($"{target.Unit.Info.Name} of {target.Unit.Info.Faction} is hit for {damageRoll}!");
        yield return slot.CurrentUnit.TakeDamage(damageRoll);

        if (slot.CurrentUnit.Unit.IsDead())
        {
            yield return slot.CurrentUnit.AnimateDeath();
            ClearCombatant(target);
        }
    }

    private void ClearCombatant(Combatant combatant)
    {
        combatant.CombatFormationSlot.ClearContents();
        _combatants.Remove(combatant);
    }

    private Winner CheckWinner()
    {
        if (!_combatants.Any(a => a.Left))
        {
            return Winner.Right;
        } else if (!_combatants.Any(a => !a.Left))
        {
            return Winner.Left;
        }

        return Winner.None;
    }

    private IEnumerable<Combatant> FindTargets(Combatant combatant)
    {
        var enemies = _combatants.Where(a => a.Left != combatant.Left);

        // Single attack
        var enemy = enemies.ToList().GetRandom();
        return enemy == null ? new List<Combatant>() : new List<Combatant>() { enemy };
    }

    private List<Combatant> GetCombatants(List<Unit> units)
    {
        if (_combatants.Any(a => a.Unit == null) || units.Any(a => a == null))
        {
            //
        }

        return _combatants.Where(a => units.Any(b => a.Unit.ID == b.ID)).ToList();
    }
}