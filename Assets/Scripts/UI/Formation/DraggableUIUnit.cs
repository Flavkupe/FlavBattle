using FlavBattle.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUIUnit : Draggable, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private bool _isSelected = false;

    public static DraggableUIUnit Selected { get; private set; }

    public Unit Unit { get; private set; }

    public event EventHandler<DraggableUIUnit> UnitClicked;

    private AnimatedCharacter _character = null;

    [SerializeField]
    private AnimatedCharacterVisuals _characterVisualProperties;

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
        Debug.Log("Click! 2");
        UnitClicked?.Invoke(this, this);
        this.SelectUnit();
    }

    public void SelectUnit()
    {
        _isSelected = true;
        Selected = this;
        SetIdle(false);
    }

    public void SetUnit(Unit unit)
    {
        Unit = unit;
        
        if (_character != null)
        {
            Destroy(_character.gameObject);
        }

        _character = Instantiate(unit.Data.AnimatedCharacter, this.transform, false);
        _character.SetVisuals(_characterVisualProperties);
        SetIdle(true);
    }

    private void SetIdle(bool idle)
    {
        _character.SetIdle(idle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
