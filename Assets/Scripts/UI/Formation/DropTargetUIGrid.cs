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

    public event EventHandler<Army> ArmyModified;

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
        Army.Formation.MoveUnit(e.Unit.Unit, e.EndingPos);
        ArmyModified?.Invoke(this, Army);
        UpdateArmy();
    }
}
