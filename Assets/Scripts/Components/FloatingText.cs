using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof (TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    public float RaisingSpeed = 10.0f;
    public float Lifetime = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        
        Destroy(this.gameObject, Lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        var shift = RaisingSpeed * Time.deltaTime;
        var newY = this.transform.position.y + shift;
        this.transform.position = this.transform.position.SetY(newY);
    }

    public void SetText(string text, Color? color = null)
    {
        var tmp = GetComponent<TextMeshPro>();
        tmp.text = text;
        if (color != null)
        {
            tmp.color = color.Value;
        }
    }
}
