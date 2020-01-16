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
}

public enum CombatTargetPriority
{
    Random,
    Closest,
    BackFirst,
    FrontFirst,
    MiddleFirst,
    Weakest,
    Strongest,
}

[CreateAssetMenu(fileName = "Strategy", menuName = "Custom/Strategies/Combat Strategy Data", order = 1)]
public class CombatStrategyData : ScriptableObject
{
    [ReorderableList]
    public CombatActionStrategy[] DefaultStrategy = new CombatActionStrategy[] {
        CombatActionStrategy.Attack,
        CombatActionStrategy.Defend,
        CombatActionStrategy.Flee,
        CombatActionStrategy.Idle
    };

    [ReorderableList]
    public CombatTargetPriority[] DefaultTargetPriority = new CombatTargetPriority[] {
        CombatTargetPriority.Random
    };
}
