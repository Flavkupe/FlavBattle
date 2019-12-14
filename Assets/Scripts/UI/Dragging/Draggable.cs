using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Draggable : MonoBehaviour, IDraggable
{
    public MonoBehaviour Instance => this;

    private Vector3 _startPos;
    private Transform _startParent;
    private bool _foundTarget = false;

    public void DropOnTarget(GameObject gameObj)
    {
        if (gameObj != null) {
            _foundTarget = true;
            transform.SetParent(gameObj.transform);
            transform.localPosition = Vector3.zero;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _foundTarget = false;
        _startPos = this.transform.position;
        _startParent = this.transform.parent;

        // Must place as sibling so that draw order appears above all else
        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling();

        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
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
    }
}
