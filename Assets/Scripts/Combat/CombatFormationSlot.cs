using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFormationSlot : MonoBehaviour
{
    public FormationRow Row;
    public FormationColumn Col;

    public CombatUnit CurrentUnit { get; private set; }

    public FormationPair GetFormation()
    {
        return new FormationPair
        {
            Row = this.Row,
            Col = this.Col
        };
    }

    public void SetUnit(CombatUnit unit)
    {
        CurrentUnit = unit;
        unit.transform.SetParent(this.transform);
        unit.transform.localPosition = new Vector3(0.0f, 0.25f, 0.0f);
    }

    public void ClearContents()
    {
        if (CurrentUnit != null)
        {
            Destroy(CurrentUnit.gameObject);
            CurrentUnit = null;
        }
    }
}
