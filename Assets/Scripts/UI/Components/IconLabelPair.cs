using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A text label and an Image, to be used in UI. Use IconTextPair for
/// GameObjects.
/// </summary>
public class IconLabelPair : MonoBehaviour, IIconTextPair
{
    public Image Icon;
    public TextMeshProUGUI Text;

    public TooltipSource Tooltip;

    public void SetText(string text)
    {
        Text.text = text;
    }

    public void SetTooltip(string text)
    {
        if (Tooltip != null)
        {
            Tooltip.TooltipText = text;
        }
    }

    public void SetIcon(Sprite sprite)
    {
        Icon.sprite = sprite;
    }
}
