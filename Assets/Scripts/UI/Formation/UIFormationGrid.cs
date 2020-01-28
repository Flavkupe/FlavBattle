using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIFormationGrid : UIUnitGridBase, IPointerClickHandler
{
    public UIFormationGridTile TileTemplate;

    public event EventHandler<UIFormationGrid> GridClicked;

    public event EventHandler<UIFormationGrid> GridRightClicked;

    public GameObject SelectionFrame;

    private bool _selected;

    protected override IFormationGridSlot OnCreateSlot()
    {
        return Instantiate(TileTemplate);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GridClicked?.Invoke(this, this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GridRightClicked?.Invoke(this, this);
        }
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        if (SelectionFrame != null)
        {
            SelectionFrame.SetActive(selected);
        }
    }
}
