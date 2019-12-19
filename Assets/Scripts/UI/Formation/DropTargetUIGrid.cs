using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropTargetUIGrid : UIUnitGridBase
{

    public DropTargetUIGridTile TileTemplate;

    private List<DropTargetUIGridTile> _tiles = new List<DropTargetUIGridTile>();

    public event EventHandler<Unit> UnitClicked;

    public event EventHandler<IArmy> ArmyModified;

    public event EventHandler<Unit> UnitReplaced;

    public event EventHandler<Unit> UnitDeployed;

    protected override IFormationGridSlot OnCreateSlot()
    {
        var tile = Instantiate(TileTemplate);
        _tiles.Add(tile);
        tile.UnitDropped += HandleUnitDropped;
        return tile;
    }

    protected override void OnAfterArmyUpdated()
    {
        var draggableUnits = GetComponentsInChildren<DraggableUIUnit>();
        foreach (var unit in draggableUnits)
        {
            unit.UnitClicked += UnitClicked;
        }
    }

    private void HandleUnitDropped(object sender, DropTargetUIGridTile.DropUnitEventArgs e)
    {
        var fromFormation = e.Unit.Unit.IsInFormation;
        var other = Army.Formation.MoveUnit(e.Unit.Unit, e.EndingPos);
        if (other != null && !other.IsInFormation)
        {
            // Dropped over other unit which is no longer
            // in army formation
            UnitReplaced?.Invoke(this, other);
        }

        if (!fromFormation)
        {
            // Unit removed from garrison
            UnitDeployed?.Invoke(this, e.Unit.Unit);
        }

        ArmyModified?.Invoke(this, Army);
        UpdateFormation();
    }
}
