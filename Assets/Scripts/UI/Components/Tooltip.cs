using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : SingletonObject<Tooltip>
{
    public TMPro.TextMeshProUGUI Text;

    public Image Icon;

    public float HorizontalPadding = 20.0f;
    public float VerticalOffset = -30.0f;

    private void Awake()
    {
        SetSingleton(this);
        this.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Input.mousePosition;
        this.transform.position = this.transform.position.ShiftY(VerticalOffset);
    }

    public void SetText(string text, Sprite icon = null)
    {
        this.Text.SetText(text);
        this.Text.ForceMeshUpdate(true);
        var width = this.Text.textBounds.size.x;
        var textWidth = width;

        if (icon != null)
        {
            Icon.Show();
            Icon.sprite = icon;
            width += 24; // icon width
            width += 16; // Padding on either side
        }
        else
        {
            Icon.sprite = null;
            Icon.Hide();
        }

        var rect = this.GetComponent<RectTransform>();
        var size = rect.sizeDelta;
        rect.sizeDelta = new Vector2(width + HorizontalPadding, size.y);

        var d = this.Text.rectTransform.sizeDelta;
        this.Text.rectTransform.sizeDelta = new Vector2(textWidth, d.y);
    }
}
