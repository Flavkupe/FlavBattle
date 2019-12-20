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

    public DraggableUnitProvider DraggableUnitProvider;

    private Mode _mode;

    private ArmyPanel _armyPanel;

    private UnitPanel _unitPanel;

    private GameEventManager _gameEvents;

    private IArmy _currentArmy;

    public void Awake()
    {
        Debug.Assert(DraggableUnitProvider != null);

        _unitPanel = GetComponentInChildren<UnitPanel>();
        _armyPanel = GetComponentInChildren<ArmyPanel>();
        _armyPanel.ArmyClicked += ArmyPanelArmyClicked;

        _unitPanel.DraggableProvider = _unitPanel.DraggableProvider ?? DraggableUnitProvider;
        Grid.DraggableProvider = Grid.DraggableProvider ?? DraggableUnitProvider;
        DraggableUnitProvider.DraggableCreated += HandleDraggableUnitCreated;

        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
        Grid.UnitDropped += HandleUnitDroppedIntoGrid;
        _unitPanel.Hide();
        _unitPanel.UnitDropped += HandleUnitDroppedIntoUnitPanel;
        _gameEvents = FindObjectOfType<GameEventManager>();
    }

    private void HandleDraggableUnitCreated(object sender, DraggableUIUnit e)
    {
        e.UnitClicked += HandleDraggableUnitClicked;
    }

    private void HandleDraggableUnitClicked(object sender, DraggableUIUnit e)
    {
        UnitStats.Show();
        UnitStats.SetUnit(e.Unit);
    }

    private void HandleUnitDroppedIntoUnitPanel(object sender, Unit e)
    {
        if (_currentArmy == null)
        {
            Debug.LogWarning("_currentArmy not set when expected!");
            return;
        }

        if (e.IsInFormation)
        {
            _currentArmy.Formation.RemoveUnit(e);
            OnArmyModified(_currentArmy);

            _gameEvents.TriggerUnitGarrisoned(e);
            _unitPanel.AddUnit(e);
        }
    }

    private void HandleUnitDroppedIntoGrid(object sender, DropUnitEventArgs e)
    {
        var fromFormation = e.Unit.Unit.IsInFormation;
        var other = e.Army.Formation.MoveUnit(e.Unit.Unit, e.EndingPos);
        if (other != null && !other.IsInFormation)
        {
            // Dropped over other unit which is no longer in army formation
            OnUnitReplaced(other);
        }

        if (!fromFormation)
        {
            // Unit removed from garrison
            _gameEvents.TriggerUnitDeployed(e.Unit.Unit);
            _unitPanel.RemoveUnit(e.Unit.Unit);
        }

        OnArmyModified(e.Army);
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

    private void OnUnitReplaced(Unit e)
    {
        UnitReplaced?.Invoke(this, e);
        var draggable = DraggableUnitProvider.GetDraggableForUnit(e);
        if (draggable != null)
        {
            // Find draggable and put into panel
            _unitPanel.DropUnit(draggable);
        }
        else
        {
            // Did not find matching DraggableUIUnit instance.
            // State should never happen, but if it does, add unit (safety)
            Debug.Assert(false, "Replaced unit with no DraggableUI; How did this state happen?");
            _unitPanel.AddUnit(e);
        }
    }

    private void OnArmyModified(IArmy e)
    {
        ArmyModified?.Invoke(this, e);
        _armyPanel.UpdatePanelContents();
        Grid.UpdateFormation();
    }

    private void HandleUnitClicked(object sender, Unit e)
    {
        UnitStats.Show();
        UnitStats.SetUnit(e);
    }

    public void SetArmy(IArmy army)
    {
        _currentArmy = army;
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
            SetArmy(armies[0]);
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
