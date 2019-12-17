using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyEditWindow : MonoBehaviour
{
    public DropTargetUIGrid Grid;

    public UnitStatsPanel UnitStats;

    public event EventHandler<Army> ArmyModified;

    public void Awake()
    {
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
        Grid.UnitClicked += HandleUnitClicked;
        Grid.ArmyModified += HandleArmyModified; ;
    }

    private void HandleArmyModified(object sender, Army e)
    {
        ArmyModified?.Invoke(this, e);
    }

    private void HandleUnitClicked(object sender, Unit e)
    {
        UnitStats.Show();
        UnitStats.SetUnit(e);
    }

    public void SetArmy(Army army)
    {
        Grid.SetArmy(army);
    }
}
