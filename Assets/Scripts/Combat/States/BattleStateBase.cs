using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BattleStateBase : IBattleState
{
    public abstract bool ShouldUpdate(BattleStatus state);
    protected abstract IEnumerator Run(BattleStatus state);

    protected MonoBehaviour _owner;

    public BattleStateBase(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void Update(BattleStatus state)
    {
        if (state.TurnExecuting)
        {
            return;
        }

        state.TurnExecuting = true;
        _owner.StartCoroutine(Execute(state));
    }

    private IEnumerator Execute(BattleStatus state)
    {
        yield return Run(state);
        state.TurnExecuting = false;
    }

    protected IEnumerator DoOfficerActions(BattleStatus state, Combatant combatant)
    {
        var actions = combatant.Unit.Info.OfficerAbilities.Where(a => a.TriggerType == OfficerAbilityTriggerType.AutoStartInCombat).ToList();
        if (actions.Count > 0)
        {
            // TODO: run each (in parallel...?) or pick a better one?
            var action = actions.GetRandom();
            yield return DoOfficerAbility(state, combatant, action);
        }
        else
        {
            Debug.Log($"{combatant.Unit.Info.Faction}: officer {combatant.Unit.Info.Name} has no officer actions!");
        }
    }

    /// <summary>
    /// Performs an officer ability based on OfficerAbilityData, such as when clicking on an action or
    /// due to events like combat start.
    /// </summary>
    protected IEnumerator DoOfficerAbility(BattleStatus state, Combatant combatant, OfficerAbilityData officerAbility)
    {
        var ability = officerAbility.CombatAbility;
        var strat = state.GetStrat(combatant);
        var targets = state.GetCombatants(strat.PickTargets(ability));
        Debug.Log($"{combatant.Unit.Info.Faction}: {combatant.Unit.Info.Name} is doing officer action {ability.Name}!");

        // TODO: other multipliers
        var multiplier = officerAbility.MultiplierType ==
            OfficerAbilityEffectMultiplierType.Constant ? officerAbility.ConstantEffectMultiplier : 1.0f;
        yield return UseAbility(state, combatant, ability, targets, multiplier);
    }

    protected IEnumerator UseAbility(BattleStatus state, Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        if (ability == null)
        {
            Debug.Log("No ability available!");
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
                yield return UseAbilityOnAllies(state, combatant, ability, targets, multiplier);
            }
            else
            {
                yield return UseAbilityOnEnemies(state, combatant, ability, targets, multiplier);
            }
        }
        else
        {
            Debug.Log("Running ability with no target");
            yield return AnimateAbility(combatant, ability);
        }

        yield return PlayAdditionalAnimations(combatant, ability.PostAttackAnimations, targets);
    }

    private IEnumerator UseAbilityOnAllies(BattleStatus state, Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        var routines = Routine.CreateEmptyRoutineSet(_owner, ability.AnimationSequence == CombatAnimationType.Parallel);
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
            else if (ability.Effect == CombatAbilityEffect.Withdraw)
            {
                state.FleeingArmy = combatant.Allies;
            }
        }

        yield return routines;
    }

    private IEnumerator UseAbilityOnEnemies(BattleStatus state, Combatant combatant, CombatAbilityData ability, List<Combatant> targets, float multiplier = 1.0f)
    {
        foreach (var target in targets)
        {
            yield return AnimateAbility(combatant, target, ability, Color.red);
            yield return AttackTarget(state, combatant, target, ability, multiplier);
        }
    }

    private IEnumerator AnimateAbility(Combatant source, CombatAbilityData abilityData)
    {
        yield return AnimateAbility(source, null, abilityData);
    }

    private IEnumerator AnimateAbility(Combatant source, Combatant target, CombatAbilityData abilityData, Color? tileHighlight = null)
    {
        if (abilityData.VisualEffect == CombatAbilityVisual.None)
        {
            yield break;
        }

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

    /// <summary>
    /// Deals morale damage to entire army (target) based on factors. source is
    /// opposing army (that is dealing morale damage). source and target can be null,
    /// depending on attack.
    /// </summary>
    private void DealMoraleDamageToArmy(BattleStatus state, IArmy source, IArmy target, int unitMoraleDamage, bool unitDied)
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
                state.BattleUIPanel.AnimateMoraleBar(source == state.PlayerArmy, true);
            }
        }

        if (target != null)
        {
            // Negative morale change for attacked army
            target.Morale.ChangeMorale(-armyDamage);
            state.BattleUIPanel.AnimateMoraleBar(target == state.PlayerArmy, false);
        }

        state.BattleUIPanel.UpdateMorale(state.PlayerArmy, state.OtherArmy);
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

    private IEnumerator AttackTarget(BattleStatus state, Combatant attacker, Combatant target, CombatAbilityData ability, float multiplier = 1.0f)
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
            state.ClearCombatant(target);
        }

        DealMoraleDamageToArmy(state, attacker?.Allies, target?.Allies, moraleDamage, unitDied);
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

        var routines = Routine.CreateEmptyRoutineSet(_owner, animationsData.Type == CombatAnimationType.Parallel);

        foreach (var animation in animationsData.Animations)
        {
            // TODO: duration type
            var animationList = new List<IPlayableAnimation>();
            if (animation.Target == CombatAnimationTarget.Self)
            {
                var instance = GameObject.Instantiate(animation.Animation);
                animationList.Add(instance);
                instance.transform.position = combatant.CombatUnit.transform.position;
            }
            else if (animation.Target == CombatAnimationTarget.Target)
            {
                foreach (var target in targets)
                {
                    var instance = GameObject.Instantiate(animation.Animation);
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
            yield return routines.AsRoutine();
        }
    }

    /// <summary>
    /// Hides all UI for combat (Buttons, backdrop, etc)
    /// </summary>
    protected IEnumerator HideCombatUI(BattleStatus state)
    {
        state.BattleUIPanel.Hide();
        yield return state.BattleDisplay.HideCombatScene();
    }
}
