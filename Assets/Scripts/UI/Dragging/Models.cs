using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IDragHandler, IBeginDragHandler, IEndDragHandler
{
    void DroppedOnTarget(DropTarget gameObj);

    MonoBehaviour Instance { get; }
}