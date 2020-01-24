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

    [BoxGroup("Targets")]
    [ShowIf("IsTargetedAbility")]
    public FormationGroup PreferredTargets;

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

    public bool IsTargetedAbility()
    {
        return Type != CombatAbilityType.Idle;
    }

    private bool ShowAnimationProps()
    {
        return VisualEffect == CombatAbilityVisual.Animation;
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

    private bool ShowProjectileArcProps()
    {
        return VisualEffect == CombatAbilityVisual.Projectile &&
            ProjectileEffect == CombatAbilityProjectileEffect.Arc;
    }

    private bool ShowProjectileVisualEffect()
    {
        return VisualEffect == CombatAbilityVisual.Projectile;
    }
}
