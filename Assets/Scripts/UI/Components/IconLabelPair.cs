using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconLabelPair : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Text;

    public void SetText(string text)
    {
        Text.text = text;
    }

    public void SetIcon(Sprite sprite)
    {
        Icon.sprite = sprite;
    }
}
