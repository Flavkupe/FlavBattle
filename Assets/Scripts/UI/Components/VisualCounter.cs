using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

/// <summary>
/// A counter that uses images to show amount
/// </summary>
public class VisualCounter : MonoBehaviour
{
    [Required]
    public Sprite Icon;

    public int Width = 32;
    public int Height = 32;

    public void SetCount(int count)
    {
        this.transform.DestroyChildren();
        for (var i = 0; i < count; i++)
        {
            var icon = new GameObject("Counter");
            var image = icon.AddComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2(Width, Height);
            image.sprite = Icon;
            icon.transform.SetParent(this.transform, false);
        }
    }
}
