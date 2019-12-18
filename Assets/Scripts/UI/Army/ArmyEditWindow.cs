using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyEditWindow : MonoBehaviour
{
    public DropTargetUIGrid Grid;

    public UnitStatsPanel UnitStats;

    public event EventHandler<IArmy> ArmyModified;

    public void Awake()
    {
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
        Grid.UnitClicked += HandleUnitClicked;
        Grid.ArmyModified += HandleArmyModified; ;
    }

    private void HandleArmyModified(object sender, IArmy e)
    {
        ArmyModified?.Invoke(this, e);
    }

    private void HandleUnitClicked(object sender, Unit e)
    {
        UnitStats.Show();
        UnitStats.SetUnit(e);
    }

    public void SetArmy(IArmy army)
    {
        Grid.SetArmy(army);
    }
}
