using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public void AddArmy(IArmy army)
    {
        _armies.Add(army);
        var grid = FormationUtils.CreateFormationGrid(ArmyGridTemplate, FormationOrientation.BottomRight, 50.0f);
        grid.transform.SetParent(ScrollContent.transform);
        grid.SetArmy(army);
        grid.GridClicked += HandleGridClicked;
        grid.GridRightClicked += HandleGridRightClicked;
        _grids.Add(grid);
    }

    private void HandleGridRightClicked(object sender, UIFormationGrid grid)
    {
        ArmyEditRequested?.Invoke(this, grid.Army);
    }

    private void HandleGridClicked(object source, UIFormationGrid grid)
    {
        ArmyClicked?.Invoke(this, grid.Army);
    }
}
