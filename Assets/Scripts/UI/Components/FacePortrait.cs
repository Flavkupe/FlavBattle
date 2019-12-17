using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FacePortrait : MonoBehaviour
{
    public Image Frame;
    public Image Portrait;

    
    public void SetUnit(Unit unit)
    {
        if (unit == null)
        {
            Frame.Hide();
            Portrait.Hide();
        }
        else
        {
            Frame.Show();
            Portrait.Show();

            Portrait.sprite = unit.Info.Portrait;
        }
    }
}
