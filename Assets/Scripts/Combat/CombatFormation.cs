using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FlavBattle.Formation;

public class CombatFormation : FormationGridBase
{
    private List<CombatFormationSlot> _slots = new List<CombatFormationSlot>();

    public CombatFormationSlot SlotTemplate;

    public override IFormationGridSlot CreateSlot()
    {
        var slot = Instantiate(SlotTemplate);
        _slots.Add(slot);
        slot.FacingLeft = FacingLeft;
        return slot;
    }

    public bool FacingLeft = false;

    [SerializeField]
    private float _gridXGap = 1.5f;

    [SerializeField]
    private float _gridYGap = 0.375f;

    private IArmy _army;

    void Awake()
    {
        var orientation = FacingLeft ? FormationOrientation.BottomLeft : FormationOrientation.BottomRight;
        FormationUtils.PopulateFormationGrid(this, orientation, _gridXGap, _gridYGap);
    }

    public void InitArmy(IArmy army)
    {
        _army = army;

        foreach (var formation in army.Formation.GetOccupiedPositions())
        {
            var unit = army.Formation.GetUnit(formation.Row, formation.Col);
            var slot = _slots.First(a => a.Col == formation.Col && a.Row == formation.Row);
            slot.SetUnit(unit);
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
