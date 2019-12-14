using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraggableUIUnit : Draggable
{
    public Unit Unit { get; private set; }

    public void SetUnit(Unit unit)
    {
        Unit = unit;
        var image = GetComponent<Image>();
        image.sprite = unit.Data.Sprite;
    }
}
