using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPanel : DropTarget
{
    public DraggableUnitProvider DraggableProvider;

    public GameObject Contents;

    private List<Unit> _units = new List<Unit>();

    public event EventHandler<Unit> UnitDropped;

    // Start is called before the first frame update
    void Start()
    {
        var dropTarget = GetComponent<DropTarget>();
        if (dropTarget != null)
        {
            dropTarget.ObjectDropped += ObjectDroppedIn;
        }
    }

    public void DropUnit(DraggableUIUnit draggable)
    {
        draggable.transform.SetParent(Contents.transform);
        if (!_units.Contains(draggable.Unit))
        {
            _units.Add(draggable.Unit);
        }
    }

    public void AddUnit(Unit unit)
    {
        var draggable = DraggableProvider.GetOrCreateDraggableForUnit(unit);
        draggable.transform.SetParent(Contents.transform, false);
        _units.Add(unit);
    }

    public void SetUnits(Unit[] units)
    {
        _units.Clear();

        foreach (var unit in units)
        {
            AddUnit(unit);
        }
    }

    public void ObjectDroppedIn(object source, IDraggable draggable)
    {
        var unit = draggable.Instance.GetComponent<DraggableUIUnit>();
        if (unit != null)
        {
            UnitDropped?.Invoke(this, unit.Unit);
            DropUnit(unit);
        }
    }

    public void RemoveUnit(Unit unit)
    {
        _units.Remove(unit);
    }

    protected override bool CanDrop(IDraggable draggable)
    {
        var unit = draggable.Instance.GetComponent<DraggableUIUnit>();
        if (unit != null)
        {
            if (unit.Unit.IsOfficer)
            {
                // Cannot drop officers into unit panel
                return false;
            }
        }

        return base.CanDrop(draggable);
    }
}