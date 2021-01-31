using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IIconTextPair
{
    void SetText(string text);
    void SetIcon(Sprite sprite);
}

/// <summary>
/// A text label and an Image, to be used for game objects.
/// Use IconLabelPair for UI.
/// 
/// Does not use a tooltip.
/// </summary>
public class IconTextPair : MonoBehaviour, IIconTextPair
{
    [SerializeField]
    private SpriteRenderer _icon;

    [SerializeField]
    private TextMeshPro _text;

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void SetIcon(Sprite sprite)
    {
        _icon.sprite = sprite;
    }

    public void FlipText()
    {
        this._text.transform.rotation = Quaternion.Euler(0, 180.0f, 0);
    }
}
