using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string TooltipText;

    [Tooltip("Optional Icon that will show up to the right of tooltip (at 24x24 pixels size)")]
    public Sprite Icon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Tooltip.Instance == null)
        {
            Debug.LogWarning("No tooltip instance in Scene!");
            return;
        }

        Tooltip.Instance.Show();
        Tooltip.Instance.SetText(TooltipText, Icon);
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
