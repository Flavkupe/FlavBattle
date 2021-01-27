using FlavBattle.Formation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UIUnitGridBase : FormationGridBase
{
    private List<IFormationGridSlot> _slots = new List<IFormationGridSlot>();

    protected List<IFormationGridSlot> Slots
    {
        get
        {
            if (_preinitialized && _slots.Count == 0)
            {
                foreach (var slot in GetComponentsInChildren<IFormationGridSlot>(true))
                {
                    _slots.Add(slot);
                }
            }

            return _slots;
        }
    }

    public IArmy Army { get; private set; }

    [Tooltip("If true, it means the grid may already have been initialized and should be read on start.")]
    [SerializeField]
    private bool _preinitialized = false;

    public override IFormationGridSlot CreateSlot()
    {
        var slot = OnCreateSlot();
        Slots.Add(slot);
        return slot;
    }

    public IEnumerable<T> GetSlotsAs<T>() where T : MonoBehaviour, IFormationGridSlot
    {
        return Slots.Select(b => b.Instance.GetComponent<T>())
                     .Where(c => c != null);
    }

    public T GetSlot<T>(FormationRow row, FormationColumn col) where T : MonoBehaviour, IFormationGridSlot
    {
        var slot = Slots.FirstOrDefault(a => a.Row == row && a.Col == col);
        return slot.Instance.GetComponent<T>();
    }

    protected abstract IFormationGridSlot OnCreateSlot();

    public void UpdateFormation()
    {
        foreach (var slot in Slots)
        {
            slot.SetUnit(null);
        }

        if (Army == null)
        {
            return;
        }

        foreach (var unit in Army.Formation.GetUnits())
        {
            var slot = Slots.FirstOrDefault(a => a.Matches(unit.Formation.Row, unit.Formation.Col));
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
