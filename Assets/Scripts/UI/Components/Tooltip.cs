using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : SingletonObject<Tooltip>
{
    public TMPro.TextMeshProUGUI Text;

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

    public void SetText(string text)
    {
        this.Text.text = text;
        // var width = this.Text.GetRenderedValues(true).x;
        this.Text.ForceMeshUpdate();
        var width = this.Text.textBounds.size.x; 
        var rect = this.GetComponent<RectTransform>();
        var size = rect.sizeDelta;
        rect.sizeDelta = new Vector2(width + HorizontalPadding, size.y);
    }
}
