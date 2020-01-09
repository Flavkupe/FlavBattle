using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum CombatAbilityTarget
{
    AllEnemies,
    RandomEnemy,
    AllAllies,
    RandomAlly
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

[CreateAssetMenu(fileName = "Combat Ability Data", menuName = "Custom/Abilities/Combat Ability Data", order = 1)]
public class CombatAbilityData : ScriptableObject
{
    public string Name;

    public CombatAbilityTarget Target;

    public FormationGroup ValidTargets;

    public FormationGroup PreferredTargets;

    public CombatAbilityVisual VisualEffect;

    [ShowIf("ShowProjectileVisualEffect")]
    public CombatAbilityProjectileEffect ProjectileEffect;

    [EnumFlags]
    public CombatAbilityEffect Effect;

    [ShowIf("ShowDamage")]
    [MinMaxSlider(0.0f, 100.0f)]
    public Vector2 Damage;

    [ShowIf("ShowMoraleDamage")]
    public int MoraleDamage;

    private bool ShowDamage()
    {
        return Effect.HasFlag(CombatAbilityEffect.Damage);
    }

    private bool ShowMoraleDamage()
    {
        return Effect.HasFlag(CombatAbilityEffect.MoraleDown);
    }

    private bool ShowProjectileVisualEffect()
    {
        return VisualEffect == CombatAbilityVisual.Projectile;
    }
}
