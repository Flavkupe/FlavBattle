using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraggableUnitProvider : MonoBehaviour
{
    public DraggableUIUnit Template;

    public event EventHandler<DraggableUIUnit> DraggableCreated;

    private List<DraggableUIUnit> _draggables = new List<DraggableUIUnit>();

    public void InitializeUnits(IEnumerable<Unit> units)
    {
        ClearAll();
        foreach (var unit in units)
        {
            CreateUnit(unit);
        }
    }

    public DraggableUIUnit CreateUnit(Unit unit)
    {
        var draggable = Instantiate(Template);

        // set Z closer to camera for raycasting
        draggable.transform.localPosition = Vector3.zero.SetZ(-1.0f);
        draggable.SetUnit(unit);
        var tooltipSource = draggable.GetComponent<TooltipSource>();
        tooltipSource.TooltipText = unit.UnitName;
        _draggables.Add(draggable);
        DraggableCreated?.Invoke(this, draggable);
        return draggable;
    }

    public DraggableUIUnit GetOrCreateDraggableForUnit(Unit unit)
    {
        var draggable = GetDraggableForUnit(unit);
        if (draggable == null)
        {
            return CreateUnit(unit);
        }

        return draggable;
    }

    public DraggableUIUnit GetDraggableForUnit(Unit unit)
    {
        var draggable = _draggables.FirstOrDefault(a => a.Unit == unit);
        return draggable;
    }

    public void ClearSpecific(DraggableUIUnit draggable)
    {
        if (draggable != null)
        {
            if (_draggables.Contains(draggable))
            {
                _draggables.Remove(draggable);
            }
                
            Destroy(draggable.gameObject);
        }
    }

    public void ClearSpecific(Unit unit)
    {
        var draggable = GetDraggableForUnit(unit);
        ClearSpecific(draggable);
    }

    public void ClearAll()
    {
        foreach (var item in _draggables)
        {
            Destroy(item.gameObject);
        }

        _draggables.Clear();
    }

}
