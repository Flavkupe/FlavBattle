using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the interactive DropTargetUIGridTile, which can handle
/// dropped things
/// </summary>
public class DropTargetUIGridTile : MonoBehaviour, IFormationGridSlot
{
    public DraggableUnitProvider DraggableProvider;

    public DraggableUIUnit DraggableUnit { get; private set; }

    public MonoBehaviour Instance => this;

    public FormationRow Row { get; set; }
    public FormationColumn Col { get; set; }

    public event EventHandler<DropUnitEventArgs> UnitDropped;

    private void Start()
    {
        Debug.Assert(DraggableProvider != null);

        var dropTarget = GetComponent<DropTarget>();
        if (dropTarget != null)
        {
            dropTarget.ObjectDropped += UnitDraggedIn;
        }
    }

    public void SetUnit(Unit unit)
    {
        if (DraggableUnit != null)
        {
            DraggableProvider.ClearSpecific(DraggableUnit);
            DraggableUnit = null;
        }

        if (unit != null)
        {
            var draggableUnit = DraggableProvider.GetOrCreateDraggableForUnit(unit);
            AttachUnit(draggableUnit);
        }
    }

    public void UnitDraggedIn(object source, IDraggable draggable)
    {
        if (draggable != null)
        {
            var unit = draggable.Instance.GetComponent<DraggableUIUnit>();
            if (unit != null)
            {
                AttachUnit(unit);
                UnitDropped?.Invoke(this, new DropUnitEventArgs
                {
                    Unit = unit,
                    StartingPos = unit.Unit.Formation,
                    EndingPos = new FormationPair(Row, Col),
                    ReplacedUnit = DraggableUnit,
                });
            }
        }
    }

    public void AttachUnit(DraggableUIUnit unit)
    {
        DraggableUnit = unit;
        unit.transform.SetParent(this.transform);
        unit.transform.localPosition = Vector3.zero;
    }
}
