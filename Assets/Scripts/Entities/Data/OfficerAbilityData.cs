using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum OfficerAbilityTriggerType
{
    /// <summary>
    /// Always active
    /// </summary>
    Passive,

    /// <summary>
    /// Player must click for action in combat
    /// </summary>
    PlayerActivatedInCombat,

    /// <summary>
    /// Automatically triggered at start of combat
    /// </summary>
    AutoStartInCombat,

    // TODO: other types
}

public enum OfficerAbilityEffectMultiplierType
{
    None,

    Constant,

    // TODO: by stats, etc
}


[CreateAssetMenu(fileName = "Officer Ability Data", menuName = "Custom/Abilities/Officer Ability Data", order = 1)]
public class OfficerAbilityData : ScriptableObject
{
    public string Name;

    [AssetIcon]
    public Sprite Icon;

    public int MinLevel = 0;

    /// <summary>
    /// How many command points the ability costs to activate
    /// </summary>
    public int CommandCost = 0;

    /// <summary>
    /// Min morale required to use. 0 if no morale min requirement.
    /// </summary>
    public int MoraleMin = 0;

    /// <summary>
    /// Max morale to use ability. 100 if no maximum to use ability.
    /// </summary>
    public int MoraleMax = 100;

    public OfficerAbilityTriggerType TriggerType;

    public bool IsCombatAbility() => TriggerType == OfficerAbilityTriggerType.AutoStartInCombat || TriggerType == OfficerAbilityTriggerType.PlayerActivatedInCombat;

    private bool UsesConstantMultiplier() => MultiplierType == OfficerAbilityEffectMultiplierType.Constant;

    [ShowIf("IsCombatAbility")]
    public CombatAbilityData CombatAbility;

    [ShowIf("IsCombatAbility")]
    public CombatTargetInfo Target;

    [ShowIf("IsCombatAbility")]
    public OfficerAbilityEffectMultiplierType MultiplierType;

    [ShowIf("UsesConstantMultiplier")]
    public float ConstantEffectMultiplier = 1.0f;
}
