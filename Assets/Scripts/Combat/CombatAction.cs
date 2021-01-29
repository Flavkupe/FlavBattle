using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum CombatAbilityPriority
{
    LastResort = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Top = 4,
}

[Flags]
public enum CombatAbilityRequiredStance
{
    Neutral = 1,
    Offensive = 2,
    Defensive = 4,
    Any = 7,
}

/// <summary>
/// What the unit can choose to do in combat
/// </summary>
[Serializable]
public class CombatAction
{
    [Tooltip("Priority in which this choice will be made")]
    public CombatAbilityPriority Priority;

    public string Description;

    public CombatTargetInfo Target;

    [Tooltip("Each bout, Instant abilities are used before non-instant abilities in combat.")]
    public bool InstantAbility = false;

    [Tooltip("The ability that will be used if the conditions are met")]
    public CombatAbilityData Ability;

    public CombatAbilityRequiredStance RequiredStance = CombatAbilityRequiredStance.Any;
}