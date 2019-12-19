using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Color _startColor;
    private Image _image;

    public event EventHandler<IDraggable> ObjectDropped;
    public Color DragHoverColor;

    public bool ChangeColorOnHover;

    void Start()
    {
        _image = GetComponent<Image>();
        if (_image != null)
        {
            _startColor = _image.color;
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        this.SetColor(_startColor);
        if (eventData.pointerDrag != null)
        {
            var draggable = eventData.pointerDrag.GetComponent<IDraggable>();
            if (draggable != null)
            {
                ObjectDropped?.Invoke(this, draggable);
                draggable.DropOnTarget(this.gameObject);

            }
        }
    }
    
    protected virtual void OnAfterDroppedTarget(IDraggable target)
    {
    }

    private void SetColor(Color color)
    {
        if (_image != null)
        {
            _image.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ChangeColorOnHover && Draggable.DraggedObject != null)
        {
            this.SetColor(DragHoverColor);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.SetColor(_startColor);
    }
}
