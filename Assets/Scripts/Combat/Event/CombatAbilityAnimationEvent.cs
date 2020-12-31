using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Performing an attack or ability; this would be the animation itself,
/// before damage or effects are applied
/// </summary>
public class CombatAbilityAnimationEvent : ICombatAnimationEvent
{
    public CombatAttackInfo _info { get; private set; }

    private AnimationType _type;

    private MonoBehaviour _owner;

    public enum AnimationType
    {
        PreAttack,
        Ability,
        PostAttack,
    }

    public CombatAbilityAnimationEvent(MonoBehaviour owner, CombatAttackInfo info, AnimationType type)
    {
        _info = info;
        _type = type;
        _owner = owner;
    }

    public IEnumerator Animate()
    {
        var targets = _info.Targets.Select(a => a.Combatant).ToList();
        var combatant = _info.Source.Combatant;

        if (_type == AnimationType.PreAttack)
        { 
            yield return PlayAdditionalAnimations(combatant, _info.Ability.PreAttackAnimations, targets);
        }
        else if (_type == AnimationType.PostAttack)
        {
            yield return PlayAdditionalAnimations(combatant, _info.Ability.PostAttackAnimations, targets);
        }
        else if (_type == AnimationType.Ability)
        {
            yield return PlayAbilityAnimation(_info.State, combatant, _info.Ability, _info, targets);
        }
    }

    private IEnumerator PlayAbilityAnimation(BattleStatus state, Combatant combatant, CombatAbilityData ability, CombatAttackInfo info, List<Combatant> targets)
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

        if (targets.Count > 0)
        {
            // Targets
            Debug.Log($"Targets: {string.Join(", ", targets.Select(a => a.Unit.Data.ClassName)) }");

            if (info.TargetInfo.AffectsAllies())
            {
                yield return UseAbilityOnAllies(state, combatant, ability, targets);
            }
            else
            {
                yield return UseAbilityOnEnemies(state, combatant, ability, targets);
            }
        }
        else
        {
            Debug.Log("Running ability with no target");
            yield return AnimateAbility(combatant, ability);
        }
    }

    private IEnumerator UseAbilityOnAllies(BattleStatus state, Combatant combatant, CombatAbilityData ability, List<Combatant> targets)
    {
        var routines = Routine.CreateEmptyRoutineSet(_owner, ability.AnimationSequence == CombatAnimationType.Parallel);
        foreach (var target in targets)
        {
            var routine = AnimateAbility(combatant, target, ability, Color.blue).ToRoutine();
            routines.AddRoutine(routine);
        }

        yield return routines;
    }

    private IEnumerator UseAbilityOnEnemies(BattleStatus state, Combatant combatant, CombatAbilityData ability, List<Combatant> targets)
    {
        foreach (var target in targets)
        {
            yield return AnimateAbility(combatant, target, ability, Color.red);
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
}
