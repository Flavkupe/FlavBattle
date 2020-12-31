using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public enum BarType
    {
        Bar,
        Radial,
    }

    public BarType Type;

    private bool IsBarType() => Type == BarType.Bar;

    [Tooltip("The colored graphic of the health bar")]
    public SpriteRenderer Bar;

    [HideIf("IsBarType")]
    [Tooltip("The sprite mask, for the Radial mask type")]
    public SpriteMask Mask;

    [ShowIf("IsBarType")]
    [Tooltip("The actual visual component that will shrink as health changes, for Bar type (does not affect colliders etc)")]
    public GameObject Visual;

    public void SetHP(int hp, float percent)
    {
        Debug.Assert(percent >= 0.0f && percent <= 1.0f);

        if (percent > 0.75f)
        {
            this.Bar.color = Color.green;
        }
        else if (percent >= 0.5f)
        {
            this.Bar.color = Color.yellow;
        }
        else
        {
            this.Bar.color = Color.red;
        }

        if (Type == BarType.Bar)
        {
            Visual.transform.localScale = Visual.transform.localScale.SetX(percent);
        }
        else if (Type == BarType.Radial)
        {
            Mask.alphaCutoff = Mathf.Max(0.01f, 1.0f - percent);
        }

        var tooltip = GetComponent<TooltipSource>();
        if (tooltip != null)
        {
            tooltip.TooltipText = hp.ToString();
        }
    }
}
