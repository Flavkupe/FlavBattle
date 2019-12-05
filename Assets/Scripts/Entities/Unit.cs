using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public FormationPair Formation;

    public UnitData Data;

    public UnitInfo Info;

    public bool IsDead()
    {
        return this.Info.CurrentStats.HP <= 0;
    }
}
