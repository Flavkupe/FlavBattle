using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject TopLevelDrag;

    public event EventHandler<IArmy> ArmyModified;
    public event EventHandler<Unit> UnitReplaced;

    private Mode _mode;

    private ArmyPanel _armyPanel;

    private UnitPanel _unitPanel;

    private GameEventManager _gameEvents;

    public void Awake()
    {
        _unitPanel = GetComponentInChildren<UnitPanel>();
        _armyPanel = GetComponentInChildren<ArmyPanel>();
        _armyPanel.ArmyClicked += ArmyPanelArmyClicked;
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
        Grid.UnitClicked += HandleUnitClicked;
        Grid.ArmyModified += HandleArmyModified;
        Grid.UnitReplaced += HandleUnitReplaced;
        Grid.UnitDeployed += HandleUnitDeployed;
        _unitPanel.Hide();

        _gameEvents = FindObjectOfType<GameEventManager>();
    }

    private void OnEnable()
    {
        Draggable.TopLevelDrag = TopLevelDrag;
    }

    private void OnDisable()
    {
        Draggable.TopLevelDrag = null;
    }

    private void ArmyPanelArmyClicked(object sender, IArmy e)
    {
        SetArmy(e);
    }

    private void HandleUnitDeployed(object sender, Unit e)
    {
        _gameEvents.TriggerUnitDeployed(e);
    }

    private void HandleUnitReplaced(object sender, Unit e)
    {
        UnitReplaced?.Invoke(this, e);
        var draggable = GetComponentsInChildren<DraggableUIUnit>().FirstOrDefault(a => a.Unit == e);
        if (draggable != null)
        {
            // Find and destroy draggable object
            // TODO: can this part be made a bit better?
            Destroy(draggable.gameObject);
        }

        _unitPanel.AddUnit(e);
    }

    private void HandleArmyModified(object sender, IArmy e)
    {
        ArmyModified?.Invoke(this, e);
        _armyPanel.UpdatePanelContents();
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
        if (armies.Length > 0)
        {
            Grid.SetArmy(armies[0]);
        }
    }

    public void SetUnitPanelContents(Unit[] units)
    {
        _unitPanel.SetUnits(units);
    }

    public void SetUnitMode()
    {
        this._unitPanel.Show();
        this._armyPanel.Hide();
    }

    public void SetArmyMode()
    {
        this._unitPanel.Hide();
        this._armyPanel.Show();
    }
}
