using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using FlavBattle.Combat;
using FlavBattle.Resources;
using FlavBattle.Components;
using FlavBattle.Combat.Animation;

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
    BackToSource,
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

[CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Combat Ability Data", order = 1)]
public class CombatAbilityData : ScriptableObject
{
    public string Name => _displayName.Text;

    [SerializeField]
    private StringResource _displayName;

    public CombatAbilityType Type = CombatAbilityType.Attack;

    [AssetIcon]
    public Sprite Icon;

    /***** Graphs ******/

    public CombatAnimationGraph AnimationGraph;

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
    [Tooltip("Possible sounds to play when attack hits target (as target flashes and takes damage)")]
    public AudioClip[] HitSoundClips;

    public bool IsTargetedAbility()
    {
        return Type != CombatAbilityType.Idle;
    }

    public bool MatchesOther(CombatAbilityData other)
    {
        // For now the criteria will be if they have the same name
        return other.Name == this.Name;
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
}
