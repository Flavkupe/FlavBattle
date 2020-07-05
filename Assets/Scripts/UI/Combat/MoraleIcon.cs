using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(TooltipSource))]
public class MoraleIcon : MonoBehaviour
{
    public void UpdateIcon(Morale morale)
    {
        var rend = GetComponent<SpriteRenderer>();
        rend.color = morale.GetColor();

        var tooltip = GetComponent<TooltipSource>();
        tooltip.TooltipText = morale.Current.ToString();
    }
}
