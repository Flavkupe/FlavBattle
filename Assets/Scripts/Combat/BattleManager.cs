using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class BattleStatus
{
    public IArmy Left;
    public IArmy Right;
}

public class BattleManager : MonoBehaviour
{
    private enum State
    {
        NotInCombat,
        AwaitingTurn,
        TurnInProgress,
        ShowWinner,

        PreCombatOfficerActions,
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
    }

    private State _state = State.NotInCombat;
    private IArmy _player;
    private IArmy _other;
    private List<Combatant> _combatants = new List<Combatant>();
    private Queue<Combatant> _turnQueue = new Queue<Combatant>();
    private GameEventManager _gameEventManager;

    public BattleDisplay BattleDisplay;

    public BattleStatus GetBattleStatus()
    {
        return new BattleStatus
        {
            Left = _player,
            Right = _other
        };
    }

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

        if (_state == State.PreCombatOfficerActions)
        {
            _state = State.TurnInProgress;
            StartCoroutine(DoPrebattleOfficerActions());
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
        _combatants.AddRange(CreateCombatants(_player, _other, true));
        _combatants.AddRange(CreateCombatants(_other, _player, false));
        _state = State.PreCombatOfficerActions;
    }

    private IEnumerable<Combatant> CreateCombatants(IArmy allies, IArmy enemies, bool left)
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

    private CombatStrategy GetStrat(Combatant combatant)
    {
        var stratData = combatant.Unit.GetStrategy();
        return new CombatStrategy(stratData, combatant.Unit, combatant.Allies, combatant.Enemies);
    }

    private IEnumerator DoPrebattleOfficerActions()
    {
        // TODO: enemy army as well
        var combatants = GetCombatants(_player.Formation.GetUnits());
        var officer = combatants.First(a => a.Unit.IsOfficer);

        yield return DoOfficerActions(officer);

        _state = State.AwaitingTurn;
    }

    private IEnumerator DoTurn(Combatant combatant)
    {
        var strat = GetStrat(combatant);
        var decision = strat.Decide();

        var ability = decision.Ability;
        var targets = GetCombatants(decision.Targets);

        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name}'s turn!");

        // TODO: multiplier
        yield return UseAbility(combatant, ability, targets);

        _state = State.AwaitingTurn;
    }

    private IEnumerator DoOfficerActions(Combatant combatant)
    {
        var strat = GetStrat(combatant);
        var actions = combatant.Unit.Info.OfficerAbilities.Where(a => a.TriggerType == OfficerAbilityTriggerType.AutoStartInCombat).ToList();
        if (actions.Count > 0)
        {
            Debug.Log(actions.Count);

            // TODO: run each (in parallel...?) or pick a better one?
            var action = actions.GetRandom();
            Debug.Log(action.Name);

            var ability = action.CombatAbility;

            Debug.Log(ability.Name);
            var targets = GetCombatants(strat.PickTargets(ability));
            Debug.Log(targets.Count);
            Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name} is doing officer action {action.CombatAbility.Name}!");

            // TODO: other multipliers
            var multiplier = action.MultiplierType == 
                OfficerAbilityEffectMultiplierType.Constant ? action.ConstantEffectMultiplier : 1.0f;
            yield return UseAbility(combatant, ability, targets, multiplier);
        }
        else
        {
            Debug.Log($"{combatant.Unit.Info.Faction}: officer {combatant.Unit.Info.Name} has no officer actions!");
        }
    }

    private IEnumerator UseAbility(Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        if (ability == null)
        {
            Debug.Log("No ability available!");
            _state = State.AwaitingTurn;
            yield break;
        }
        else
        {
            Debug.Log($"Using ability {ability.Name}");
        }

        yield return PlayAdditionalAnimations(combatant, ability.PreAttackAnimations, targets);

        if (targets.Count > 0)
        {
            // Targets
            Debug.Log($"Targets: {string.Join(", ", targets.Select(a => a.Unit.Data.ClassName)) }");

            if (ability.AffectsAllies())
            {
                yield return UseAbilityOnAllies(combatant, ability, targets, multiplier);
            }
            else
            {
                yield return UseAbilityOnEnemies(combatant, ability, targets, multiplier);
            }
        }
        else
        {
            Debug.Log("Running ability with no target");
            yield return AnimateAbility(combatant, ability);
        }

        yield return PlayAdditionalAnimations(combatant, ability.PostAttackAnimations, targets);

        // yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Plays CombatCharacterAnimations on targets or combatant, based on data.
    /// For exmaple, PreAttackAnimations or PostAttackAnimations, which is just
    /// the animations before or after combat.
    /// 
    /// Will run to completion if option is enabled. Otherwise will run in background.
    /// </summary>
    /// <param name="combatant">Unit whose turn it is</param>
    /// <param name="animationsData">Data for this set of animations, such as PreAttackAnimations or PostAttackAnimations</param>
    /// <param name="targets">Targets of ability, if any</param>
    private IEnumerator PlayAdditionalAnimations(Combatant combatant, CombatCharacterAnimations animationsData, List<Combatant> targets)
    {
        if (animationsData.Animations.Count() == 0)
        {
            yield break;
        }

        var routines = Routine.CreateEmptyRoutineSet(this, animationsData.Type == CombatAnimationType.Parallel);

        foreach (var animation in animationsData.Animations)
        {
            // TODO: duration type
            var animationList = new List<IPlayableAnimation>();
            if (animation.Target == CombatAnimationTarget.Self)
            {
                var instance = Instantiate(animation.Animation);
                animationList.Add(instance);
                instance.transform.position = combatant.CombatUnit.transform.position;
            }
            else if (animation.Target == CombatAnimationTarget.Target)
            {
                foreach (var target in targets)
                {
                    var instance = Instantiate(animation.Animation);
                    animationList.Add(instance);
                    instance.transform.position = target.CombatUnit.transform.position;
                }
            }

            foreach (var anim in animationList)
            {
                if (animationsData.WaitForCompletion)
                {
                    routines.AddRoutine(anim.PlayToCompletion().ToRoutine());
                }
                else
                {
                    anim.PlayAnimation();
                }
            }
        }

        if (animationsData.WaitForCompletion)
        {
            yield return routines;
        }
    }

    private IEnumerator UseAbilityOnAllies(Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        var routines = Routine.CreateEmptyRoutineSet(this, ability.AnimationSequence == CombatAnimationType.Parallel);
        foreach (var target in targets)
        {
            var routine = AnimateAbility(combatant, target, ability, Color.blue).ToRoutine();
            routines.AddRoutine(routine);

            // TODO: other effects
            if (ability.Effect == CombatAbilityEffect.StatusChange)
            {
                var effect = ability.StatusEffect.Multiply(multiplier);
                target.ApplyStatChanges(effect);
            }
        }

        yield return routines;
    }

    private IEnumerator UseAbilityOnEnemies(Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        var stats = GetCombinedCombatantStats(combatant);
        var damageRoll = stats.Power;

        foreach (var target in targets)
        {
            // TODO: other effects
            var damage = damageRoll;
            damage += ability.Damage.RandomBetween();
            damage = (int)((float)damage * multiplier);
            yield return AnimateAbility(combatant, target, ability, Color.red);
            Debug.Log($"Damage roll for {damage}!");
            yield return AttackTarget(combatant, target, damage);
        }
    }

    private IEnumerator AnimateAbility(Combatant source, CombatAbilityData abilityData)
    {
        yield return AnimateAbility(source, null, abilityData);
    }

    private IEnumerator AnimateAbility(Combatant source, Combatant target, CombatAbilityData abilityData, Color? tileHighlight = null)
    {
        CombatFormationSlot slot = null;
        if (target != null)
        {
            slot = target.CombatFormationSlot;
            slot.Highlight(tileHighlight ?? Color.white);
        }

        var obj = new GameObject("Ability");
        var ability = obj.AddComponent<CombatAbility>();

        ability.InitData(abilityData);

        if (target != null)
        {
            yield return ability.StartTargetedAbility(source.CombatFormationSlot.CurrentUnit.gameObject, target.CombatFormationSlot.CurrentUnit.gameObject);
        }
        else
        {
            yield return ability.StartUntargetedAbility(source.CombatFormationSlot.CurrentUnit.gameObject);
        }

        if (slot != null)
        {
            slot.ResetColor();
        }
    }

    private IEnumerator AttackTarget(Combatant attacker, Combatant target, int damageRoll)
    {
        var slot = target.CombatFormationSlot;
        Debug.Log($"{target.Unit.Info.Name} of {target.Unit.Info.Faction} is hit for {damageRoll}!");

        
        var targetStats = GetCombinedCombatantStats(target);
        var mitigation = targetStats.Defense;

        Debug.Log($"Total damage mitigation from target: {mitigation} for total damage of {damageRoll}");

        damageRoll = Math.Max(1, damageRoll - mitigation);

        yield return slot.CurrentUnit.TakeDamage(damageRoll);

        if (slot.CurrentUnit.Unit.IsDead())
        {
            yield return slot.CurrentUnit.AnimateDeath();
            ClearCombatant(target);
        }
    }
    /// <summary>
    /// Gets all combined target stats (with buffs etc) for combatant
    /// </summary>
    private UnitStats GetCombinedCombatantStats(Combatant combatant)
    {
        var targetStats = combatant.Unit.Info.CurrentStats;
        targetStats = targetStats.Combine(combatant.StatChanges);
        return targetStats;
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
        return _combatants.Where(a => units.Any(b => a.Unit.ID == b.ID)).ToList();
    }
}