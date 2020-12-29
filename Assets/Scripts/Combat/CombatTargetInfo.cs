using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum CombatAbilityTarget
{
    Self,
    AllEnemies,
    RandomEnemy,
    AllAllies,
    RandomAlly,
}

/// <summary>
/// Specific opponent requirement for this ability to work
/// </summary>
public enum ValidOpponent
{
    /// <summary>
    /// Works on any opponent
    /// </summary>
    Any,

    /// <summary>
    /// Only works on lower level opponent
    /// </summary>
    LowerLevel,

    /// <summary>
    /// Only works on higher level opponent
    /// </summary>
    HigherLevel,
}

[Serializable]
public class CombatTargetInfo
{
    public CombatAbilityTarget TargetType = CombatAbilityTarget.RandomEnemy;

    [AllowNesting]
    [ShowIf("IsTargetedAbility")]
    public ValidOpponent ValidOpponent = ValidOpponent.Any;

    [AllowNesting]
    [ShowIf("IsTargetedAbility")]
    public FormationGroup ValidTargets;

    public bool AffectsAllies()
    {
        return TargetType == CombatAbilityTarget.AllAllies ||
            TargetType == CombatAbilityTarget.RandomAlly;
    }

    public bool IsTargetedAbility()
    {
        return TargetType == CombatAbilityTarget.RandomEnemy ||
            TargetType == CombatAbilityTarget.RandomAlly;
    }
}

