using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropTargetUIGrid : UIUnitGridBase
{
    public DraggableUnitProvider DraggableProvider;

    public DropTargetUIGridTile TileTemplate;

    private List<DropTargetUIGridTile> _tiles = new List<DropTargetUIGridTile>();

    public event EventHandler<DropUnitEventArgs> UnitDropped;

    protected override IFormationGridSlot OnCreateSlot()
    {
        var tile = Instantiate(TileTemplate);
        _tiles.Add(tile);
        tile.UnitDropped += HandleUnitDropped;
        tile.DraggableProvider = DraggableProvider;
        return tile;
    }

    private void HandleUnitDropped(object sender, DropUnitEventArgs e)
    {
        e.Army = this.Army;
        UnitDropped?.Invoke(this, e);
    }

    public void SetUnitSelected(Unit unit)
    {
        if (unit == null)
        {
            return;
        }

        foreach (var tile in _tiles)
        {
            var tileUnit = tile?.DraggableUnit?.Unit;
            if (tileUnit != null && tileUnit.Equals(unit))
            {
                tile.SelectUnit();
                return;
            }
        }
    }
}
