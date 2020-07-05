using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Morale
{
    public int Current = 100;

    public void ChangeMorale(int change)
    {
        Current += change;
    }

    public Color GetColor()
    {
        return Color.Lerp(Color.red, Color.green, (float)Current / 100.0f);
    }
}
