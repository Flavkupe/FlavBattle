using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IDragHandler, IBeginDragHandler, IEndDragHandler
{
    void DropOnTarget(GameObject gameObj);

    MonoBehaviour Instance { get; }
}