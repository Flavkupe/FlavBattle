using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSource : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string TooltipText;

    public Tooltip Tooltip;

    void Start()
    {
        if (Tooltip == null)
        {
            var holder = GetComponentInParent<TooltipHolder>();
            if (holder == null || holder.Tooltip == null)
            {
                Debug.LogError("No tooltips or tooltip holder found!!");
            }
            else
            {
                Tooltip = holder.Tooltip;
            }
        }
    }

    [Tooltip("Optional Icon that will show up to the right of tooltip (at 24x24 pixels size)")]
    public Sprite Icon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Tooltip == null)
        {
            Debug.LogWarning("No tooltip instance in Scene!");
            return;
        }

        Tooltip.Show();
        Tooltip.SetText(TooltipText, Icon);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip == null)
        {
            Debug.LogWarning("No tooltip instance in Scene!");
            return;
        }

        Tooltip.Hide();
    }
}
