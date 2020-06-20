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

    private SpriteRenderer _sprite;
    private Color _startColor;

    public GameObject OfficerRing;

    public FormationPair GetFormation()
    {
        return new FormationPair
        {
            Row = this.Row,
            Col = this.Col
        };
    }

    private void Start()
    {
        OfficerRing.Hide();
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite != null)
        {
            _startColor = _sprite.color;
        }
    }

    public void SetUnit(Unit unit)
    {
        // Put it in the .25f mark, adjusting for scale of slot
        var yPos = 0.25f / transform.localScale.y;
        var combatUnit = Instantiate(CombatUnitTemplate);
        combatUnit.name = unit.Info.Name;
        combatUnit.SetUnit(unit, FacingLeft);
        CurrentUnit = combatUnit;
        combatUnit.transform.SetParent(this.transform);
        combatUnit.transform.localPosition = new Vector3(0.0f, yPos, 0.0f);

        if (unit.IsOfficer)
        {
            OfficerRing.Show();
        }
        else
        {
            OfficerRing.Hide();
        }
    }

    public void ClearContents()
    {
        if (CurrentUnit != null)
        {
            Destroy(CurrentUnit.gameObject);
            CurrentUnit = null;
        }
    }

    public void Highlight(Color color)
    {
        if (_sprite != null)
        {
            _sprite.color = color;
        }
    }

    public void ResetColor()
    {
        if (_sprite != null)
        {
            _sprite.color = _startColor;
        }
    }
}
