using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Draggable : MonoBehaviour, IDraggable
{
    /// <summary>
    /// Current object set at the top of the draw stack to ensure this draws on top of everything
    /// </summary>
    public static GameObject TopLevelDrag { get; set; }

    public static Draggable DraggedObject { get; private set; }

    public MonoBehaviour Instance => this;

    private Vector3 _startPos;
    private Transform _startParent;
    private bool _foundTarget = false;

    public void DroppedOnTarget(DropTarget target)
    {
        if (target != null && target.gameObject != null)
        {
            _foundTarget = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _foundTarget = false;
        _startPos = this.transform.position;
        _startParent = this.transform.parent;

        
        if (TopLevelDrag == null)
        {
            // Must place as sibling so that draw order appears above all else
            transform.SetParent(transform.parent.parent);
            transform.SetAsLastSibling();
        }
        else
        {
            transform.SetParent(TopLevelDrag.transform); 
        }

        this.GetComponent<CanvasGroup>().blocksRaycasts = false;

        DraggedObject = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_foundTarget)
        {
            transform.position = _startPos;
            transform.SetParent(_startParent);
        }

        this.GetComponent<CanvasGroup>().blocksRaycasts = true;

        DraggedObject = null;
    }
}
