using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public float RaisingSpeed = 10.0f;
    public float Lifetime = 1.0f;

    [Tooltip("Time it will stay frozen without rising after Lifetime. If 0, will not do so.")]
    public float PauseTime = 0.0f;

    private float life = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        Destroy(this.gameObject, Lifetime + PauseTime);
    }

    void Update()
    {
        life += TimeUtils.FullAdjustedGameDelta;
        if (life < Lifetime)
        {
            var shift = RaisingSpeed * TimeUtils.FullAdjustedGameDelta;
            var newY = this.transform.position.y + shift;
            this.transform.position = this.transform.position.SetY(newY);
        }
    }

    public void SetText(string text, Color? color = null)
    {
        var tmp = GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = text;
            if (color != null)
            {
                tmp.color = color.Value;
            }
        }

        var tmp2 = GetComponent<Text>();
        if (tmp2 != null)
        {
            tmp2.text = text;
            if (color != null)
            {
                tmp2.color = color.Value;
            }
        }
    }
}
