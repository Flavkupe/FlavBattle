using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum CombatActionStrategy
{
    Attack,
    Defend,
    Flee,
    Idle,
    Any,
}

public enum CombatTargetPriority
{
    Random,
    Closest,

    // Prefers back two rows over front row
    BackFirst,

    // Prefers front row over back two rows
    FrontFirst,
    Weakest,
    Strongest,
}

public interface ICombatStrategy
{
    CombatActionStrategy[] Strategies { get; }
    CombatTargetPriority TargetPriority { get; }

    /// <summary>
    /// An ability that happens when nothing else can happen, such
    /// as an idle wait.
    /// </summary>
    CombatAbilityData DefaultAbility { get; }

}

[CreateAssetMenu(fileName = "Strategy", menuName = "Custom/Strategies/Combat Strategy Data", order = 1)]
public class CombatStrategyData : ScriptableObject, ICombatStrategy
{
    [ReorderableList]
    public CombatActionStrategy[] DefaultStrategy = new CombatActionStrategy[] {
        CombatActionStrategy.Attack,
        CombatActionStrategy.Defend,
        CombatActionStrategy.Flee,
        CombatActionStrategy.Idle
    };

    public CombatTargetPriority DefaultTargetPriority = CombatTargetPriority.Random;

    public CombatActionStrategy[] Strategies => DefaultStrategy;

    public CombatTargetPriority TargetPriority => DefaultTargetPriority;

    [SerializeField]
    [Required]
    private CombatAbilityData _defaultAbility;

    public CombatAbilityData DefaultAbility => _defaultAbility;
}
