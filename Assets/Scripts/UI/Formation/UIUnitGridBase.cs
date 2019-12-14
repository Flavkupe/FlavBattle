using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UIUnitGridBase : MonoBehaviour, IFormationGrid
{
    public Army Army { get; private set; }

    private List<IFormationGridSlot> _slots = new List<IFormationGridSlot>();

    public IFormationGridSlot CreateSlot()
    {
        var slot = OnCreateSlot();
        _slots.Add(slot);
        return slot;
    }

    protected abstract IFormationGridSlot OnCreateSlot();

    public void UpdateArmy()
    {
        foreach (var slot in _slots)
        {
            slot.SetUnit(null);
        }

        if (Army == null)
        {
            return;
        }

        foreach (var unit in Army.Formation.GetUnits())
        {
            var slot = _slots.FirstOrDefault(a => a.Matches(unit.Formation.Row, unit.Formation.Col));
            if (slot == null)
            {
                Debug.LogWarning("Error matching unit to drop grid!");
                continue;
            }

            slot.SetUnit(unit);
        }
    }

    public void SetArmy(Army army)
    {
        Army = army;
        UpdateArmy();
    }
}
