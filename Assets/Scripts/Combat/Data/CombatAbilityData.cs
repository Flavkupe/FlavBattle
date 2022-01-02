using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using FlavBattle.Combat;
using FlavBattle.Resources;

public enum CombatAbilityType
{
    Attack,
    Defense,
    Idle,
}

[Flags]
public enum CombatAbilityEffect
{
    Damage = 1,
    Heal = 2,
    MoraleDown = 4,
    StatusChange = 8,
    Withdraw = 16,
}

public enum CombatAbilityVisual
{
    Projectile,
    Animation,
    
    /// <summary>
    /// No specific visual effect for animation, though it can
    /// still have post attack and pre-attack animations
    /// </summary>
    None,
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

public enum CombatAnimationType
{
    None,
    Serial,
    Parallel,
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

    public Props[] Animations;

    private bool ShowAnimations()
    {
        return Type != CombatAnimationType.None;
    }

    [Serializable]
    public class Props
    {
        public PlayableAnimation Animation;

        public CombatAnimationDurationType DurationType;

        public CombatAnimationTarget Target;

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
    public string Name => _displayName.Text;

    [SerializeField]
    private StringResource _displayName;

    public CombatAbilityType Type = CombatAbilityType.Attack;

    [AssetIcon]
    public Sprite Icon;

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
    [Tooltip("Whether to show the name of the attack overhead before the attack")]
    public bool AnimateName = false;

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
    [Tooltip("String name for animator for what character animation would look like (ie ShootBow). Left empty, does no animator animation.")]
    [SerializeField]
    private UnitAnimatorTrigger _animatorTriggerName = UnitAnimatorTrigger.None;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    [Tooltip("An additional multiplier over the animation's existing speed.")]
    public float CombatAnimationSpeed = 1.0f;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    public float CombatAnimationRepeats = 1.0f;

    [BoxGroup("Visuals")]
    [ShowIf("ShowAnimationProps")]
    [Tooltip("Whether to wait for the animation before continuing")]
    public bool WaitForCompletion = true;

    [BoxGroup("Visuals")]
    [ShowIf(EConditionOperator.And, "ShowAnimationProps", "IsTargetedAbility")]
    [Tooltip("Whether the animation shows up on the target or source, where applicable")]
    public CombatAnimationTarget CombatAnimationTarget;

    [BoxGroup("Visuals")]
    [ShowIf(EConditionOperator.And, "ShowAnimationProps", "IsMultitarget")]
    [Tooltip("For multiple animations, which sequence they run in")]
    public CombatAnimationType AnimationSequence;

    /***** Character Animation ******/

    [BoxGroup("Visuals")]
    public CombatCharacterAnimations PreAttackAnimations;

    [BoxGroup("Visuals")]
    public CombatCharacterAnimations PostAttackAnimations;

    /// <summary>
    /// Gets animator animation for this attack (if any)
    /// </summary>
    public UnitAnimatorTrigger AnimatorTrigger => _animatorTriggerName;

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
    [MinMaxSlider(0.0f, 100.0f)]
    public Vector2 MoraleDamage;

    [BoxGroup("Effect")]
    [ShowIf("ShowStatusChange")]
    public UnitStats StatusEffect;

    [Tooltip("How many bouts the status effect lasts. If 0, it lasts the whole combat.")]
    [BoxGroup("Effect")]
    [ShowIf("ShowStatusChange")]
    public int StatusEffectDuration = 0;

    [BoxGroup("Sounds")]
    [Tooltip("Possible sounds to play when attack starts")]
    public AudioClip[] StartSoundClips;

    [BoxGroup("Sounds")]
    [Tooltip("Possible sounds to play as soon as animator animation completes (eg drew bow, about to shoot arrow)")]
    public AudioClip[] PostAnimatorSoundClips;

    [BoxGroup("Sounds")]
    [Tooltip("Possible sounds to play right before hit animation starts (such as for swords)")]
    public AudioClip[] PreHitSoundClips;

    [BoxGroup("Sounds")]
    [Tooltip("Possible sounds to play when attack hits target (as target flashes and takes damage)")]
    public AudioClip[] HitSoundClips;

    [BoxGroup("Sounds")]
    [Tooltip("Possible sounds to play when attack finishes")]
    public AudioClip[] EndSoundClips;

    public bool IsTargetedAbility()
    {
        return Type != CombatAbilityType.Idle;
    }

    public bool MatchesOther(CombatAbilityData other)
    {
        // For now the criteria will be if they have the same name
        return other.Name == this.Name;
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
}
