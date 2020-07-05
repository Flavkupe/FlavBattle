using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public SpriteRenderer Bar;
    
    [Required]
    [Tooltip("The actual visual component that will shrink as health changes (does not affect colliders etc)")]
    public GameObject Visual;

    public void SetHP(int hp, float percent)
    {
        Debug.Assert(percent >= 0.0f && percent <= 1.0f);
        Visual.transform.localScale = Visual.transform.localScale.SetX(percent);
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

        var tooltip = GetComponent<TooltipSource>();
        if (tooltip != null)
        {
            tooltip.TooltipText = hp.ToString();
        }
    }
}
