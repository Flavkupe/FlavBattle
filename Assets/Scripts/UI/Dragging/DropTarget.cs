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

    /// <summary>
    /// Event that fires right before an object will be dropped, allowing the
    /// drop to be cancelled
    /// </summary>
    public event EventHandler<IDraggable> ObjectWillBeDropped;

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
            if (draggable != null && CanDrop(draggable))
            {
                ObjectWillBeDropped?.Invoke(this, draggable);
                if (!draggable.CancelDrag)
                {
                    ObjectDropped?.Invoke(this, draggable);
                    draggable.DroppedOnTarget(this);
                }
            }
        }
    }

    /// <summary>
    /// Allows override to determine if item should be dropped here.
    /// </summary>
    protected virtual bool CanDrop(IDraggable draggable)
    {
        return true;
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
