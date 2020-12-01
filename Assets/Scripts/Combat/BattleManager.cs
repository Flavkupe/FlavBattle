using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using NaughtyAttributes;

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

        public int UnitMorale => Unit.Info.Morale.Current;
        public int ArmyMorale => Allies.Morale.Current;
    }

    private State _state = State.NotInCombat;
    private IArmy _player;
    private IArmy _other;
    private List<Combatant> _combatants = new List<Combatant>();
    private Queue<Combatant> _turnQueue = new Queue<Combatant>();
    private GameEventManager _gameEventManager;

    private Queue<OfficerAbilityData> _abilityQueue = new Queue<OfficerAbilityData>();

    private const float BattleFormationChangeCooldownSeconds = 5.0f;

    [Required]
    public BattleDisplay BattleDisplay;

    [Required]
    public BattleUIPanel BattleUIPanel;

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

        BattleUIPanel.OnStanceChangeClicked += (object o, FightingStance stance) => HandleStanceChanged(stance, true);
        BattleUIPanel.OnCommandAbilityUsed += HandleBattleUIPanelOnCommandAbilityUsed;
        BattleUIPanel.Hide();
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

            if (this._abilityQueue.Count > 0)
            {
                // Do a queued ability
                _state = State.TurnInProgress;
                StartCoroutine(DoQueuedOfficerAbility());
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

    /// <summary>
    /// Shows the winner of combat and hides all combat state
    /// </summary>
    /// <param name="winner"></param>
    /// <returns></returns>
    private IEnumerator ShowWinner(Winner winner)
    {
        var victory = winner == Winner.Left;
        yield return BattleDisplay.ShowCombatEndSign(victory);
        yield return HideCombatUI();
        var winningArmy = victory ? _player : _other;
        var losingArmy = victory ? _other : _player;
        _gameEventManager.TriggerCombatEndedEvent(winningArmy, losingArmy);
        _state = State.NotInCombat;
    }

    /// <summary>
    /// Hides all UI for combat (Buttons, backdrop, etc)
    /// </summary>
    private IEnumerator HideCombatUI()
    {
        BattleUIPanel.Hide();
        yield return BattleDisplay.HideCombatScene();
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


        // Init UI based on army setting
        HandleStanceChanged(player.Stance, false);
        BattleUIPanel.SetArmies(_player, _other);

        // Enable UI State
        BattleUIPanel.Show();
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

    private Combatant GetPlayerOfficer()
    {
        var combatants = GetCombatants(_player.Formation.GetUnits());
        var officer = combatants.First(a => a.Unit.IsOfficer);
        return officer;
    }

    private IEnumerator DoPrebattleOfficerActions()
    {
        // TODO: enemy army as well
        var officer = GetPlayerOfficer();
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
        var actions = combatant.Unit.Info.OfficerAbilities.Where(a => a.TriggerType == OfficerAbilityTriggerType.AutoStartInCombat).ToList();
        if (actions.Count > 0)
        {
            // TODO: run each (in parallel...?) or pick a better one?
            var action = actions.GetRandom();            
            yield return DoOfficerAbility(combatant, action);
        }
        else
        {
            Debug.Log($"{combatant.Unit.Info.Faction}: officer {combatant.Unit.Info.Name} has no officer actions!");
        }
    }

    /// <summary>
    /// Dequeues an officer ability from the list and executes it
    /// </summary>
    private IEnumerator DoQueuedOfficerAbility()
    {
        var officer = GetPlayerOfficer();
        var ability = this._abilityQueue.Dequeue();
        yield return BattleUIPanel.AnimateAbilityNameCallout(ability);
        yield return this.DoOfficerAbility(officer, ability);
        _state = State.AwaitingTurn;
    }

    /// <summary>
    /// Performs an officer ability based on OfficerAbilityData, such as when clicking on an action or
    /// due to events like combat start.
    /// </summary>
    private IEnumerator DoOfficerAbility(Combatant combatant, OfficerAbilityData officerAbility)
    {
        var ability = officerAbility.CombatAbility;
        var strat = GetStrat(combatant);
        var targets = GetCombatants(strat.PickTargets(ability));
        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name} is doing officer action {ability.Name}!");

        // TODO: other multipliers
        var multiplier = officerAbility.MultiplierType ==
            OfficerAbilityEffectMultiplierType.Constant ? officerAbility.ConstantEffectMultiplier : 1.0f;
        yield return UseAbility(combatant, ability, targets, multiplier);
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
        foreach (var target in targets)
        {
            yield return AnimateAbility(combatant, target, ability, Color.red);
            yield return AttackTarget(combatant, target, ability, multiplier);
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

    private IEnumerator AttackTarget(Combatant attacker, Combatant target, CombatAbilityData ability, float multiplier = 1.0f)
    {
        var stats = GetCombinedCombatantStats(attacker);
        var slot = target.CombatFormationSlot;
        var targetStats = GetCombinedCombatantStats(target);

        var moraleDamage = 0;

        // TODO: other effects
        if (ability.Effect.HasFlag(CombatAbilityEffect.Damage))
        {
            var damage = stats.Power;
            damage += ability.Damage.RandomBetween();
            damage = (int)((float)damage * multiplier);
            Debug.Log($"Damage roll for {damage}!");
            var mitigation = targetStats.Defense;
            Debug.Log($"Total damage mitigation from target: {mitigation} for total damage of {damage}");
            damage = Math.Max(1, damage - mitigation);
            yield return slot.CurrentUnit.TakeDamage(damage);
            Debug.Log($"{target.Unit.Info.Name} of {target.Unit.Info.Faction} is hit for {damage}!");

            // do some additional morale damage
            moraleDamage = CalculateMoraleDamage(attacker, target, damage);
        }

        if (ability.Effect.HasFlag(CombatAbilityEffect.MoraleDown))
        {
            // TODO: morale damage mitigation based on bravery stats and other factors
            moraleDamage += ability.MoraleDamage.RandomBetween();

            // do and animate morale damage
            yield return slot.CurrentUnit.TakeMoraleDamage(moraleDamage, true);
        }
        else
        {
            // do the morale damage without showing it if it's not part of the effect
            yield return slot.CurrentUnit.TakeMoraleDamage(moraleDamage, false);
        }

        var unitDied = slot.CurrentUnit.Unit.IsDead();
        if (unitDied)
        {
            yield return slot.CurrentUnit.AnimateDeath();
            ClearCombatant(target);
        }

        DealMoraleDamageToArmy(attacker?.Allies, target?.Allies, moraleDamage, unitDied);
    }

    /// <summary>
    /// Deals morale damage to entire army (target) based on factors. source is
    /// opposing army (that is dealing morale damage). source and target can be null,
    /// depending on attack.
    /// </summary>
    private void DealMoraleDamageToArmy(IArmy source, IArmy target, int unitMoraleDamage, bool unitDied)
    {
        // TODO: affected by other stats
        // TODO: should mitigate under certain conditions
        var armyDamage = (int)Math.Max(1, (float)unitMoraleDamage / 5.0f);
        if (unitDied)
        {
            var roll = UnityEngine.Random.Range(5, 10);
            armyDamage += roll;
            if (source != null && armyDamage > 0)
            {
                // Positive morale change for attacking army
                source.Morale.ChangeMorale(roll);
                BattleUIPanel.AnimateMoraleBar(source == _player, true);
            }
        }

        if (target != null)
        {
            // Negative morale change for attacked army
            target.Morale.ChangeMorale(-armyDamage);
            BattleUIPanel.AnimateMoraleBar(target == _player, false);
        }

        BattleUIPanel.UpdateMorale(_player, _other);
    }

    /// <summary>
    /// Given the 2 combatants and the damage dealt, calculates and returns Morale damage
    /// taken, accounting for all things.
    /// </summary>
    private int CalculateMoraleDamage(Combatant attacker, Combatant target, int damage)
    {
        // TODO: different morale damages
        // Currently taking half of damage as morale damage
        return damage / 2;
    }

    /// <summary>
    /// Gets all combined target stats (with buffs etc) for combatant.
    /// Includes morale roll.
    /// </summary>
    private UnitStats GetCombinedCombatantStats(Combatant combatant)
    {
        var targetStats = combatant.Unit.Info.CurrentStats;
        var armyBonuses = GetArmyBonusStats(combatant.Allies);
        targetStats = targetStats.Combine(combatant.StatChanges, armyBonuses);

        // Calculate the morale effect on Combat stats.
        // Roll between -0.5 to 0.5, with 50 morale being 0. Multiply stats
        // by 1 + that number (range of 0.5 to 1.5, ie between 50% less to 50% more)
        var moraleRoll = GetCombatantMoraleRoll(combatant);
        moraleRoll -= 50;
        var moraleMultiplier = 1.0f + ((float)moraleRoll / 100.0f);
        targetStats.Multiply(moraleMultiplier);

        return targetStats;
    }

    /// <summary>
    /// Gets a roll for morale, consisting of a random point between
    /// unit morale and his Allies' morale. It's a value from 0 to 100
    /// </summary>
    private int GetCombatantMoraleRoll(Combatant combatant)
    {
        var min = Math.Min(combatant.UnitMorale, combatant.ArmyMorale);
        var max = Math.Max(combatant.UnitMorale, combatant.ArmyMorale);
        return UnityEngine.Random.Range(min, max + 1); // +1 because Range top is exclusive
    }

    /// <summary>
    /// Gets unit stat bonuses associated with army.
    /// </summary>
    private UnitStats GetArmyBonusStats(IArmy army)
    {
        var stats = new UnitStats();
        if (army.Stance == FightingStance.Defensive)
        {
            // TEMP: simple stance bonuses for now
            stats.Defense += 1;
        }
        else if (army.Stance == FightingStance.Offensive)
        {
            // TEMP: simple stance bonuses for now
            stats.Power += 1;
        }

        return stats;
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

    /// <summary>
    /// Handles a UI stance change, such as from the UI or combat start
    /// </summary>
    /// <param name="stance"></param>
    private void HandleStanceChanged(FightingStance stance, bool applyCooldown = false)
    {
        this._player.Stance = stance;
        BattleUIPanel.UpdateStance(stance, applyCooldown ? BattleFormationChangeCooldownSeconds : 0.0f);
    }

    /// <summary>
    /// Handles clicking on a command item from the CommandAbility panel.
    /// </summary>
    private void HandleBattleUIPanelOnCommandAbilityUsed(object sender, OfficerAbilityData e)
    {
        var officer = this.GetPlayerOfficer();
        if (e.CommandCost <= officer.Unit.Info.CurrentStats.Command)
        {
            this._abilityQueue.Enqueue(e);
            officer.Unit.Info.CurrentStats.Command -= e.CommandCost;
            this.BattleUIPanel.CommandMenu.UpdateMenu();
        }
    }
}
