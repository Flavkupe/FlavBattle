using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AnimatedSprite))]
public class DraggableUIUnit : Draggable, IPointerClickHandler
{
    private bool _isSelected = false;

    public static DraggableUIUnit Selected { get; private set; }

    public Unit Unit { get; private set; }

    public event EventHandler<DraggableUIUnit> UnitClicked;

    private void Update()
    {
        // Something else was selected
        if (_isSelected && Selected != this)
        {
            _isSelected = false;
            SetIdle(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UnitClicked?.Invoke(this, this);
        _isSelected = true;
        Selected = this;
        SetIdle(false);
    }

    public void SetUnit(Unit unit)
    {
        Unit = unit;
        var animation = GetComponent<AnimatedSprite>();
        animation.SetAnimations(unit.Data.Animations);
        animation.SetIdle(true);
    }

    private void SetIdle(bool idle)
    {
        var animation = GetComponent<AnimatedSprite>();
        animation.SetIdle(idle);
    }
}
