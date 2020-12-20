using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArmyMapAIBase : MonoBehaviour
{
    public int Priority = 1;

    public abstract bool IsActionPossible(Army army, TilemapManager map);

    public abstract void DoActionTick(Army army, TilemapManager map);

    public abstract bool ShouldContinueAction { get; }
}
