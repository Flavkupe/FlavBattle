using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public event EventHandler<IDraggable> ObjectDropped;

    public void OnDrop(PointerEventData eventData)
    {
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
}
