using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NextCombatantTurnState : BattleStateBase
{
    public NextCombatantTurnState(MonoBehaviour owner) : base(owner)
    {
    }

    /// <summary>
    /// Executes if there are combatants in turn queue
    /// </summary>
    public override bool ShouldUpdate(BattleStatus state)
    {
        return state.Stage == BattleStatus.BattleStage.CombatPhase && (state.TurnQueue.Count > 0 || state.AbilityQueue.Count > 0);
    }

    protected override IEnumerator Run(BattleStatus state)
    {
        if (state.AbilityQueue.Count > 0)
        {
            // Queued officer ability
            var officer = state.GetPlayerOfficer();
            var ability = state.AbilityQueue.Dequeue();
            yield return state.BattleUIPanel.AnimateAbilityNameCallout(ability);
            var action = new CombatAction()
            {
                Ability = ability.CombatAbility,
                Target = ability.Target,
            };

            yield return DoTurn(state, officer, action);
        }
        else
        {
            // Queued combatant
            var current = state.GetNextCombatant();
            if (current != null)
            {
                yield return DoTurn(state, current);
            }
        }
    }

    private IEnumerator DoTurn(BattleStatus state, Combatant combatant, CombatAction preloadedAction = null)
    {
        var action = preloadedAction ?? PickAction(state, combatant, combatant.Unit.Info.Actions);
        var targets = PickTargets(state, combatant, action.Target);

        // TODO: Step 1 should be part of an ICombatEvent
        // Step 1: calculate pre-attack info
        var info = GetAttackInfo(state, combatant, action, targets);
        var infoList = new List<CombatAttackInfo>();
        if (info != null)
        {
            infoList.Add(info);
        }

        // Update panel
        if (!info.IsAllyAbility)
        {
            state.BattleUIPanel.AttackStats.SetStats(infoList);
        }
        else
        {
            state.BattleUIPanel.AttackStats.Clear();
        }

        var preAnimation = new CombatAbilityAnimationEvent(_owner, info, CombatAbilityAnimationEvent.AnimationType.PreAttack);
        var attackAnimation = new CombatAbilityAnimationEvent(_owner, info, CombatAbilityAnimationEvent.AnimationType.Ability);
        var attackEvent = new CombatAttackEvent(_owner, info);
        var postAnimation = new CombatAbilityAnimationEvent(_owner, info, CombatAbilityAnimationEvent.AnimationType.PostAttack);

        // Step 2: pre-ability animations
        yield return preAnimation.Animate();

        // Step 3: ability animation
        yield return attackAnimation.Animate();

        // Step 4: process ability and get results (includes rolls etc)
        var results = attackEvent.Process();

        foreach (var result in results.Results)
        {
            yield return AnimateAttackResults(state, result);
        }

        // Step 5: post ability animation
        yield return postAnimation.Animate();

        // Step 6: check for newly dead units
        yield return CheckForDeadUnits(state);

        state.BattleUIPanel.UpdateMorale(state.PlayerArmy, state.OtherArmy);
    }

    private IEnumerator CheckForDeadUnits(BattleStatus state)
    {
        foreach (var combatant in state.Combatants.ToList())
        {
            
            if (combatant.Unit.IsDead())
            {
                yield return combatant.CombatUnit.AnimateDeath();
                state.ClearCombatant(combatant);

                // get morale bonus for killing unit
                var roll = UnityEngine.Random.Range(5, 10);

                // Positive morale change for attacking army
                combatant.Allies.Morale.ChangeMorale(roll);
                state.BattleUIPanel.AnimateMoraleBar(combatant.IsInPlayerArmy, true);
            }
        }
    }

    private IEnumerator AnimateAttackResults(BattleStatus state, ComputedAttackResultInfo info)
    {
        if (info.Target == null)
        {
            // Currently results with no target should probably just not animate
            yield break;
        }

        if (info.ArmyMoraleDamage.HasValue)
        {
            state.BattleUIPanel.AnimateMoraleBar(info.Target.IsInPlayerArmy, info.ArmyMoraleDamage.Value > 0);
        }

        // Prefer physical damage
        if (info.AttackDamage.HasValue)
        {
            var anim = new CombatUnitAnimationEvent(_owner, info);
            yield return anim.Animate();
        }

        if (info.DirectMoraleDamage.HasValue)
        {
            var anim = new CombatUnitAnimationEvent(_owner, info);
            yield return anim.Animate();
        }
    }

    // TODO: multiple at once
    private CombatAttackInfo GetAttackInfo(BattleStatus state, Combatant combatant, CombatAction action, List<Combatant> targets)
    {
        var info = new CombatAttackInfo();

        var combatantInfo = new ComputedAttackInfo()
        {
            Attack = GetTotalAttack(combatant, action),
            Combatant = combatant,
        };

        var targetsInfo = new List<ComputedAttackInfo>();
        foreach (var target in targets)
        {
            var targetInfo = new ComputedAttackInfo()
            {
                Defense = GetTotalDefense(target, action),
                Combatant = target,
            };

            targetsInfo.Add(targetInfo);
        }

        info.Targets = targetsInfo;
        info.Source = combatantInfo;
        info.Ability = action.Ability;
        info.TargetInfo = action.Target;
        info.State = state;
        return info;
    }

    private int GetTotalAttack(Combatant combatant, CombatAction action)
    {
        var attack = combatant.Unit.Info.CurrentStats.Power;
        attack += action.Ability.Damage.RandomBetween();
        attack += combatant.UnitMoraleBonus;
        return attack;
    }

    private int GetTotalDefense(Combatant combatant, CombatAction action)
    {
        var defense = combatant.Unit.Info.CurrentStats.Defense;
        defense += combatant.UnitMoraleBonus;
        return defense;
    }

    /// <summary>
    /// Filters abilities by those that can target enemy formation
    /// </summary>
    private List<CombatAction> FilterPossibleActions(BattleStatus state, Combatant combatant, List<CombatAction> actions)
    {
        var enemyPositions = combatant.Enemies.Formation.GetOccupiedPositions(true);
        List<CombatAction> possible = new List<CombatAction>();
        foreach (var action in actions)
        {
            // Check if ability can hit any units
            if (CanHitUnits(state, combatant, action))
            {
                possible.Add(action);
            }
        }

        return possible;
    }

    /// <summary>
    /// Checks whether any units are affected by the ability. Checks both positional
    /// and unit requirements of ability.
    /// </summary>
    private bool CanHitUnits(BattleStatus state, Combatant combatant, CombatAction ability)
    {
        return GetValidAbilityTargets(state, combatant, ability.Target).Count > 0;
    }

    /// <summary>
    /// Picks an attack by priority, if one exists. If not, then returns null
    /// (meaning there is no preference).
    /// </summary>
    private CombatAction PickAction(BattleStatus state, Combatant combatant, List<CombatAction> actions)
    {
        // First filter by possible attacks, and return default if none are possible.
        var possible = FilterPossibleActions(state, combatant, actions);

        if (possible.Count == 0)
        {
            Debug.Log("No valid actions! Returning global default");
            return GameResourceManager.Instance.GetDefaultCombatAction();
        }

        // Get random action from the top priority possible action
        var maxPriority = possible.Max(a => a.Priority);
        return possible.Where(a => a.Priority == maxPriority).ToList().GetRandom();
    }

    private List<CombatAbilityPriority> GetPriorityValuesReversed()
    {
        var list = new List<CombatAbilityPriority>();
        foreach (CombatAbilityPriority priority in Enum.GetValues(typeof(CombatAbilityPriority)))
        {
            list.Add(priority);
        }

        list.Reverse();
        return list;
    }
}

