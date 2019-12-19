using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPanel : DropTarget
{
    public DraggableUIUnit DraggableUnitTemplate;

    public GameObject Contents;

    private List<Unit> _units = new List<Unit>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddUnit(Unit unit)
    {
        var draggable = Instantiate(DraggableUnitTemplate);
        draggable.SetUnit(unit);
        draggable.transform.SetParent(Contents.transform);
        _units.Add(unit);
    }

    public void Refresh()
    {

    }

    public void SetUnits(Unit[] units)
    {
        foreach (var unit in GetComponentsInChildren<DraggableUIUnit>())
        {
            Destroy(unit.gameObject);
        }

        _units.Clear();

        foreach (var unit in units)
        {
            AddUnit(unit);
        }
    }

    protected override void OnAfterDroppedTarget(IDraggable target)
    {
        base.OnAfterDroppedTarget(target);
    }
}
