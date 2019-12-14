﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the non-interactive UIFormationGridTile
/// </summary>
public class UIFormationGridTile : MonoBehaviour, IFormationGridSlot
{
    public MonoBehaviour Instance => this;

    public FormationRow Row { get; set; }
    public FormationColumn Col { get; set; }

    public Image UnitImage;

    private Unit _unit;

    public void SetUnit(Unit unit)
    {
        this._unit = unit;
        if (unit == null)
        {
            this.UnitImage.gameObject.SetActive(false);
            this.UnitImage.sprite = null;
        }
        else
        {
            this.UnitImage.gameObject.SetActive(true);
            this.UnitImage.sprite = unit.Data.Sprite;
        }
    }
}
