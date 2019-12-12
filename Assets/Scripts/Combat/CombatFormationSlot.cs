using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFormationSlot : MonoBehaviour, IFormationGridSlot
{
    public FormationRow Row { get; set; }
    public FormationColumn Col { get; set; }

    public CombatUnit CurrentUnit { get; private set; }

    public MonoBehaviour Instance => this;

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
