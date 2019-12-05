using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public SpriteRenderer Bar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPercent(float percent)
    {
        Debug.Assert(percent >= 0.0f && percent <= 1.0f);
        this.transform.localScale = this.transform.localScale.SetX(percent);
        if (percent > 0.75f)
        {
            this.Bar.color = Color.green;
        }
        else if (percent >= 0.5f)
        {
            this.Bar.color = Color.yellow;
        }
        else
        {
            this.Bar.color = Color.red;
        }
    }
}
