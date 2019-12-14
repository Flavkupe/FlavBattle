using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the non-interactive UIFormationGridTile
/// </summary>
public class DropTargetUIGridTile : MonoBehaviour, IFormationGridSlot
{
    public class DropUnitEventArgs : EventArgs
    {
        public DraggableUIUnit Unit { get; set; }

        public FormationPair StartingPos { get; set; }

        public FormationPair EndingPos { get; set; }

        public DraggableUIUnit ReplacedUnit { get; set; }
    }

    public DraggableUIUnit DraggableUnitTemplate;

    public DraggableUIUnit DraggableUnit { get; private set; }

    public MonoBehaviour Instance => this;

    public FormationRow Row { get; set; }
    public FormationColumn Col { get; set; }

    public event EventHandler<DropUnitEventArgs> UnitDropped;

    private void Start()
    {
        var dropTarget = GetComponent<DropTarget>();
        if (dropTarget != null)
        {
            dropTarget.ObjectDropped += UnitDraggedIn;
        }
    }

    public void SetUnit(Unit unit)
    {
        if (unit == null)
        {
            if (DraggableUnit != null)
            {
                Destroy(DraggableUnit.gameObject);
                DraggableUnit = null;
            }
        }
        else
        {
            if (DraggableUnit != null)
            {
                Destroy(DraggableUnit.gameObject);
            }

            var draggableUnit = Instantiate(DraggableUnitTemplate);
            draggableUnit.SetUnit(unit);
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
