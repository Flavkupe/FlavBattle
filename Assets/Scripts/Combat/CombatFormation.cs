using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CombatFormation : MonoBehaviour
{
    public CombatUnit CombatUnitTemplate;
    private CombatFormationSlot[] _slots;

    public bool FacingLeft = false;

    private Army _army;

    void Awake()
    {
        _slots = GetComponentsInChildren<CombatFormationSlot>();
    }

    public void InitArmy(Army army)
    {
        _army = army;

        foreach (var formation in army.Formation.GetOccupiedPositions())
        {
            var unit = army.Formation.GetUnit(formation.Row, formation.Col);
            var slot = _slots.First(a => a.Col == formation.Col && a.Row == formation.Row);
            var combatUnit = Instantiate(CombatUnitTemplate);
            combatUnit.name = unit.Data.Name;
            combatUnit.SetUnit(unit, FacingLeft);
            slot.SetUnit(combatUnit);
        }
    }

    public void ClearArmy()
    {
        _army = null;
        foreach(var slot in _slots)
        {
            slot.ClearContents();
        }
    }

    public CombatFormationSlot GetFormationSlot(FormationRow row, FormationColumn col)
    {
        return _slots.First(a => a.Row == row && a.Col == col);
    }
}
