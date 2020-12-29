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

    [Tooltip("The ability that will be used if the conditions are met")]
    public CombatAbilityData Ability;
}