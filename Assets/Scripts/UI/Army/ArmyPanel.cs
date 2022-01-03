using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ArmyPanel : MonoBehaviour
{
    public UIFormationGrid ArmyGridTemplate;

    private List<IArmy> _armies = new List<IArmy>();

    private List<UIFormationGrid> _grids = new List<UIFormationGrid>();

    public event EventHandler<IArmy> ArmyClicked;

    public event EventHandler<IArmy> ArmyEditRequested;

    public GameObject ScrollContent;

    public void UpdatePanelContents()
    {
        foreach (var grid in _grids)
        {
            grid.UpdateFormation();
        }
    }

    public void Clear()
    {
        foreach (var grid in _grids)
        {
            Destroy(grid.gameObject);
        }

        _grids.Clear();
        _armies.Clear();
    }

    public void SetArmies(IArmy[] armies)
    {
        Clear();
        foreach (var army in armies)
        {
            AddArmy(army);
        }
    }

    public void AddArmy(IArmy army)
    {
        _armies.Add(army);
        var grid = FormationUtils.CreateFormationGrid(ArmyGridTemplate, 50.0f, FormationOrientation.BottomRight);
        grid.transform.SetParent(ScrollContent.transform, false);
        grid.SetArmy(army);
        grid.GridClicked += HandleGridClicked;
        grid.GridRightClicked += HandleGridRightClicked;
        grid.SetSelected(false);
        _grids.Add(grid);
    }

    public void SetSelectedArmy(IArmy army)
    {
        if (army == null)
        {
            this.SetSelected(null);
        }
        else if (army.IsPlayerOwned())
        {
            var grid = _grids.FirstOrDefault(a => a.Army.ID == army.ID);
            if (grid == null)
            {
                Debug.LogWarning("No grid to match army in ArmyPanel!");
                return;
            }

            this.SetSelected(grid);
        }
    }

    private void HandleGridRightClicked(object sender, UIFormationGrid grid)
    {
        ArmyEditRequested?.Invoke(this, grid.Army);
    }

    private void HandleGridClicked(object source, UIFormationGrid grid)
    {
        ArmyClicked?.Invoke(this, grid.Army);
        SetSelected(grid);
    }

    private void SetSelected(UIFormationGrid selected)
    {
        foreach (var grid in _grids)
        {
            // Unselect all other armies first
            grid.SetSelected(false);
        }

        if (selected != null)
        {
            selected.SetSelected(true);
        }
    }
}
