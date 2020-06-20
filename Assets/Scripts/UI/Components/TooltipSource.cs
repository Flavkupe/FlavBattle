using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string TooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.Instance.Show();
        Tooltip.Instance.SetText(TooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Instance.Hide();
    }
}
