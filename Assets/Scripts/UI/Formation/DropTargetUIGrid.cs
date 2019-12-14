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

    protected override IFormationGridSlot OnCreateSlot()
    {
        var tile = Instantiate(TileTemplate);
        _tiles.Add(tile);
        tile.UnitDropped += HandleUnitDropped;
        return tile;
    }

    private void HandleUnitDropped(object sender, DropTargetUIGridTile.DropUnitEventArgs e)
    {
        Army.Formation.MoveUnit(e.Unit.Unit, e.EndingPos);
        UpdateArmy();
    }
}
