using FlavBattle.Formation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UIUnitGridBase : FormationGridBase
{
    private List<IFormationGridSlot> _slots = new List<IFormationGridSlot>();
    

    public IArmy Army { get; private set; }

    public override IFormationGridSlot CreateSlot()
    {
        var slot = OnCreateSlot();
        _slots.Add(slot);
        return slot;
    }

    protected abstract IFormationGridSlot OnCreateSlot();

    public void UpdateFormation()
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

    /// <summary>
    /// Sets the Grid to an active army with a formation.
    /// </summary>
    public void SetArmy(IArmy army)
    {
        Army = army;
        UpdateFormation();
    }
}
