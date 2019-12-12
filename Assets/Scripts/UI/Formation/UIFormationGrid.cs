using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIFormationGrid : MonoBehaviour, IFormationGrid, IPointerClickHandler
{
    public UIFormationGridTile TileTemplate;

    public Army Army { get; private set; }

    public List<UIFormationGridTile> Slots { get; } = new List<UIFormationGridTile>();

    public event EventHandler<UIFormationGrid> GridClicked;

    public IFormationGridSlot CreateSlot()
    {
        var slot = Instantiate(TileTemplate);
        Slots.Add(slot);
        return slot;
    }

    public void UpdateArmy()
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
                Debug.LogWarning("Error matching unit to formation grid!");
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

    public void OnPointerClick(PointerEventData eventData)
    {
        GridClicked?.Invoke(this, this);
    }
}
