using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;

    public Image Icon;

    /// <summary>
    /// Whether the mouse is relative to camera coords rather
    /// than canvas coords when this tooltip appears
    /// </summary>
    public bool CameraSpace = false;

    public float HorizontalPadding = 20.0f;
    public float VerticalOffset = -30.0f;

    private TooltipSource _currentSource = null;

    private void Awake()
    {
        this.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraSpace)
        {
            this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            this.transform.position = Input.mousePosition;
        }

        this.transform.position = this.transform.position.ShiftY(VerticalOffset);
        this.transform.position = this.transform.position.SetZ(0);

        if (_currentSource != null && !_currentSource.gameObject.activeInHierarchy)
        {
            // If source goes away, hide tooltip
            this.HideTooltip();
        }
    }

    public void HideTooltip()
    {
        _currentSource = null;
        this.Hide();
    }

    public void ShowTooltip(TooltipSource source)
    {
        _currentSource = source;
        this.Show();
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
