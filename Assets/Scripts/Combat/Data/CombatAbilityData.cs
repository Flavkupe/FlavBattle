using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum CombatAbilityType
{
    Attack,
    Defense,
    Idle,
}

public enum CombatAbilityTarget
{
    AllEnemies,
    RandomEnemy,
    AllAllies,
    RandomAlly,
}

[Flags]
public enum CombatAbilityEffect
{
    Damage = 1,
    Heal = 2,
    MoraleDown = 4,
    StatusChange = 8,
}

public enum CombatAbilityVisual
{
    Projectile,
    Animation,
}

public enum CombatAbilityProjectileEffect
{
    Straight,
    Arc
}

public enum CombatAbilityCharacterMoveEffect
{
    Teleport,
    Straight,
    Arc
}

public enum CombatAbilityCharacterMoveTarget
{
    Front,
}

public enum CombatAbilityPriority
{
    LastResort = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Top = 4,
}

public enum CombatAnimationType
{
    None,
    Serial,
    Parallel,
    PickRandom,
}

public enum CombatAnimationDurationType
{
    OneCycle,
    Duration,
}

public enum CombatAnimationTarget
{
    /// <summary>
    /// Combat animation shows up on user
    /// </summary>
    Self,

    /// <summary>
    /// Combat animation shows up on target
    /// </summary>
    Target
}

[Serializable]
public class CombatCharacterAnimations
{
    public CombatAnimationType Type;

    public bool WaitForCompletion;

    [ShowIf("ShowProps")]
    public Props[] Animations;

    private bool ShowProps()
    {
        return Type != CombatAnimationType.None;
    }

    [Serializable]
    public class Props
    {
        public IPlayableAnimation Animation;

        public CombatAnimationDurationType DurationType;

        [ShowIf("ShowDuration")]
        public float Duration;

        private bool ShowDuration()
        {
            return DurationType == CombatAnimationDurationType.Duration;
        }
    }
}

[CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Combat Ability Data", order = 1)]
public class CombatAbilityData : ScriptableObject
{
    public string Name;

    public CombatAbilityType Type = CombatAbilityType.Attack;

    public CombatAbilityPriority Priority = CombatAbilityPriority.Medium;

    /***** Targets ******/

    [BoxGroup("Targets")]
    [ShowIf("IsTargetedAbility")]
    public CombatAbilityTarget Target;

    [BoxGroup("Targets")]
    [ShowIf("IsTargetedAbility")]
    public FormationGroup ValidTargets;

    /***** Visuals ******/

    [BoxGroup("Visuals")]
    public CombatAbilityVisual VisualEffect;

    /** Projectile **/

    [BoxGroup("Visuals")]
    [ShowIf("ShowProjectileVisualEffect")]
    public CombatAbilityProjectileEffect ProjectileEffect;

    [BoxGroup("Visuals")]
    [ShowIf("ShowProjectileArcProps")]
    [MinMaxSlider(-10.0f, 10.0f)]
    public Vector2 ArcHeight = new Vector2(0.0f, 0.0f);

    [BoxGroup("Visuals")]
    [ShowIf("ShowProjectileArcProps")]
    [Tooltip("Whether the projectile changes its direction along the arc")]
    public bool TraceDirection = false;

    [BoxGroup("Visuals")]
    [ShowIf("ShowProjectileVisualEffect")]
    public float ProjectileSpeed;

    [ShowAssetPreview]
    [BoxGroup("Visuals")]
    [ShowIf("ShowProjectileVisualEffect")]
    public GameObject ProjectileObject;

    /***** Movement Effect ******/

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    public bool CharacterMove;

    [BoxGroup("Visuals")]
    [ShowIf("ShowCharacterMoveProps")]
    public CombatAbilityCharacterMoveEffect CharacterMoveToEffect;

    [BoxGroup("Visuals")]
    [ShowIf("ShowCharacterMoveProps")]
    public CombatAbilityCharacterMoveEffect CharacterMoveBackEffect;

    [BoxGroup("Visuals")]
    [ShowIf("ShowCharacterMoveProps")]
    [Tooltip("Where to move relative to target")]
    public CombatAbilityCharacterMoveTarget CharacterMoveTarget;

    [BoxGroup("Visuals")]
    [ShowIf("ShowCharacterMoveArcProps")]
    public float CharacterMoveArcHeight;

    [BoxGroup("Visuals")]
    [ShowIf("ShowCharacterMoveProps")]
    public float CharacterMoveSpeed;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    public PlayableAnimation ComabtAnimation;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    [Tooltip("An additional multiplier over the animation's existing speed.")]
    public float CombatAnimationSpeed = 1.0f;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    public float CombatAnimationRepeats = 1.0f;

    [BoxGroup("Visuals")]
    [ShowIf(ConditionOperator.And, "ShowAnimationProps", "IsTargetedAbility")]
    [Tooltip("Whether the animation shows up on the target or source, where applicable")]
    public CombatAnimationTarget CombatAnimationTarget;

    [BoxGroup("Visuals")]
    [ShowIf(ConditionOperator.And, "ShowAnimationProps", "IsMultitarget")]
    [Tooltip("For multiple animations, which sequence they run in")]
    public CombatAnimationType AnimationSequence;

    /***** Character Animation ******/

    [BoxGroup("Visuals")]
    public CombatCharacterAnimations PreAttackAnimations;

    [BoxGroup("Visuals")]
    public CombatCharacterAnimations PostAttackAnimations;

    /***** Effect ******/

    [BoxGroup("Effect")]
    [EnumFlags]
    public CombatAbilityEffect Effect;

    [BoxGroup("Effect")]
    [ShowIf("ShowDamage")]
    [MinMaxSlider(0.0f, 100.0f)]
    public Vector2 Damage;

    [BoxGroup("Effect")]
    [ShowIf("ShowMoraleDamage")]
    public int MoraleDamage;

    [BoxGroup("Effect")]
    [ShowIf("ShowStatusChange")]
    public UnitStats StatusEffect;

    public bool IsTargetedAbility()
    {
        return Type != CombatAbilityType.Idle;
    }

    public bool AffectsAllies()
    {
        switch (Target)
        {
            case CombatAbilityTarget.AllAllies:
            case CombatAbilityTarget.RandomAlly:
                return true;
            default:
                return false;
        }
    }

    private bool ShowAnimationProps()
    {
        return VisualEffect == CombatAbilityVisual.Animation;
    }

    private bool IsMultitarget()
    {
        return Target == CombatAbilityTarget.AllAllies || Target == CombatAbilityTarget.AllEnemies;
    }

    private bool ShowCharacterMoveProps()
    {
        return CharacterMove;
    }

    private bool ShowCharacterMoveArcProps()
    {
        return ShowCharacterMoveProps() &&
            (CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Arc ||
            CharacterMoveBackEffect == CombatAbilityCharacterMoveEffect.Arc);
    }

    private bool ShowDamage()
    {
        return Effect.HasFlag(CombatAbilityEffect.Damage);
    }

    private bool ShowMoraleDamage()
    {
        return Effect.HasFlag(CombatAbilityEffect.MoraleDown);
    }

    private bool ShowStatusChange()
    {
        return Effect.HasFlag(CombatAbilityEffect.StatusChange);
    }

    private bool ShowProjectileArcProps()
    {
        return VisualEffect == CombatAbilityVisual.Projectile &&
            ProjectileEffect == CombatAbilityProjectileEffect.Arc;
    }

    private bool ShowProjectileVisualEffect()
    {
        return VisualEffect == CombatAbilityVisual.Projectile;
    }

    public bool MatchesStrat(CombatActionStrategy strat)
    {
        return strat == CombatActionStrategy.Any || this.Type.ToString() == strat.ToString();
    }
}
