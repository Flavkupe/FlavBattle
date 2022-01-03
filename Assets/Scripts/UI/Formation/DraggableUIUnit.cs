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
    private SortingLayerValues _sortingLayer;

    [Tooltip("Scaling factor for units instantiated for this element")]
    [SerializeField]
    private Vector3 _unitScale = new Vector3(32.0f, 32.0f, 1.0f);

    [Tooltip("Offset shift after unit is instantiated.")]
    [SerializeField]
    private Vector3 _unitOffset = new Vector3(0.0f, 16.0f, 1.0f);

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
        _character.transform.localPosition = _unitOffset;
        _character.transform.localScale = _unitScale;
        _character.SetSortingLayer(_sortingLayer.Name, _sortingLayer.Value);
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
