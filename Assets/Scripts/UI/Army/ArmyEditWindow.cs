using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyEditWindow : MonoBehaviour
{
    public enum Mode
    {
        DeployedArmy,
        Garrison
    }

    public DropTargetUIGrid Grid;

    public UnitStatsPanel UnitStats;

    public event EventHandler<IArmy> ArmyModified;

    private Mode _mode;

    private ArmyPanel _armyPanel;

    public void Awake()
    {
        _armyPanel = GetComponentInChildren<ArmyPanel>();
        _armyPanel.ArmyClicked += ArmyPanelArmyClicked;
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
        Grid.UnitClicked += HandleUnitClicked;
        Grid.ArmyModified += HandleArmyModified;
    }

    private void ArmyPanelArmyClicked(object sender, IArmy e)
    {
        SetArmy(e);
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

    public void SetMode(Mode mode = Mode.DeployedArmy)
    {
        _mode = mode;
    }

    public void SetArmyPanelContents(IArmy[] armies)
    {
        _armyPanel.SetArmies(armies);
    }
}
