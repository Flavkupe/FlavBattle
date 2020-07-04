using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string TooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Tooltip.Instance == null)
        {
            Debug.LogWarning("No tooltip instance in Scene!");
            return;
        }

        Tooltip.Instance.Show();
        Tooltip.Instance.SetText(TooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip.Instance == null)
        {
            Debug.LogWarning("No tooltip instance in Scene!");
            return;
        }

        Tooltip.Instance.Hide();
    }
}
