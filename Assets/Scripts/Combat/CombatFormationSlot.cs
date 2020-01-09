﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatFormationSlot : MonoBehaviour, IFormationGridSlot
{
    public CombatUnit CombatUnitTemplate;

    public bool FacingLeft { get; set; }

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

    public void SetUnit(Unit unit)
    {
        // Put it in the .25f mark, adjusting for scale of slot
        var yPos = 0.25f / transform.localScale.y;
        var combatUnit = Instantiate(CombatUnitTemplate);
        combatUnit.name = unit.Data.Name;
        combatUnit.SetUnit(unit, FacingLeft);
        CurrentUnit = combatUnit;
        combatUnit.transform.SetParent(this.transform);
        combatUnit.transform.localPosition = new Vector3(0.0f, yPos, 0.0f);
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
