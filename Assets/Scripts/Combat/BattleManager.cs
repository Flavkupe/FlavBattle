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
    }

    private State _state = State.NotInCombat;
    private Army _player;
    private Army _other;
    private List<Combatant> _combatants = new List<Combatant>();
    private Queue<Combatant> _turnQueue = new Queue<Combatant>();


    public BattleDisplay BattleDisplay;

    // Start is called before the first frame update
    void Start()
    {
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
                // TODO: winner
                return;
            }

            if (_turnQueue.Count > 0)
            {
                _state = State.TurnInProgress;
                StartCoroutine(DoTurn(_turnQueue.Dequeue()));
            }
            else
            {
                // Round done; rearrange turns
                ArrangeTurns();
            }
        }
    }

    public Coroutine StartCombat(Army player, Army enemy)
    {
        return StartCoroutine(StartCombatInternal(player, enemy));
    }

    private IEnumerator StartCombatInternal(Army player, Army enemy)
    {
        _player = player;
        _other = enemy;
        _combatants.Clear();
        _turnQueue.Clear();
        _combatants.AddRange(GetCombatants(_player, true));
        _combatants.AddRange(GetCombatants(_other, false));
        yield return BattleDisplay.InitializeCombatScene(player, enemy);
        _state = State.AwaitingTurn;
    }

    private IEnumerable<Combatant> GetCombatants(Army army, bool left)
    {
        return army.Formation.GetOccupiedPositionInfo().Select(a => new Combatant
        {
            Left = left,
            Unit = a.Unit,
            Row = a.FormationPair.Row,
            Col = a.FormationPair.Col,
        });
    }

    private void ArrangeTurns()
    {
        foreach(var item in _combatants.OrderBy(a => a.Unit.Info.CurrentStats.Speed).Reverse())
        {
            _turnQueue.Enqueue(item);
        }
    }

    private IEnumerator DoTurn(Combatant combatant)
    {
        var unitFormation = combatant.Left ? BattleDisplay.LeftFormation : BattleDisplay.RightFormation;
        var opponentFormation = combatant.Left ? BattleDisplay.RightFormation: BattleDisplay.LeftFormation;
        var targets = FindTargets(combatant);

        var currentUnitSlot = unitFormation.GetFormationSlot(combatant.Row, combatant.Col);

        if (targets.Count() == 0)
        {
            // TODO
            yield break;
        }

        yield return currentUnitSlot.CurrentUnit.AnimateAttack();

        foreach (var target in targets)
        {
            var slot = opponentFormation.GetFormationSlot(target.Row, target.Col);
            yield return slot.CurrentUnit.AnimateDamaged();
        }

        yield return new WaitForSeconds(0.5f);

        _state = State.AwaitingTurn;
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
}