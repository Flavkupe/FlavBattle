using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FacePortrait : MonoBehaviour
{
    public Image Portrait;
    public Image FrameNormies;
    public Image FrameOfficer;


    public void SetUnit(Unit unit)
    {
        FrameNormies.Hide();
        FrameOfficer.Hide();
        if (unit == null)
        {
            Portrait.Hide();
        }
        else
        {
            Portrait.Show();
            Portrait.sprite = unit.Info.Portrait;
            if (unit.IsOfficer)
            {
                FrameOfficer.Show();
            }
            else
            {
                FrameNormies.Show();
            }
        }
    }
}
