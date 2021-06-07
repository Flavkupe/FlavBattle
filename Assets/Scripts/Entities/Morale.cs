using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Morale
{
    public enum Tier
    {
        Panic = 1,
        Low = 2,
        Med = 3,
        High = 4,
        VeryHigh = 5,
    }

    public int Current = 100;

    public void ChangeMorale(int change)
    {
        Current += change;
        Current = Mathf.Clamp(Current, 0, 100);
    }

    public int GetTierNumber()
    {
        return (int)GetTier();
    }

    public int GetDefaultBonus()
    {
        var tier = GetTier();
        switch (tier)
        {
            case Tier.VeryHigh:
                return 2;
            case Tier.High:
                return 1;
            case Tier.Med:
                return 0;
            case Tier.Low:
                return -1;
            case Tier.Panic:
            default:
                return -2;
        }
    }

    public Tier GetTier()
    {
        if (Current > 90)
        {
            return Tier.VeryHigh;
        }

        if (Current > 75)
        {
            return Tier.High;
        }

        if (Current > 50)
        {
            return Tier.Med;
        }

        if (Current > 25)
        {
            return Tier.Low;
        }

        return Tier.Panic;
    }

    public Color GetColor()
    {
        var tier = GetTier();
        switch (tier)
        {
            case Tier.VeryHigh:
                return Color.green;
            case Tier.High:
                return Color.cyan;
            case Tier.Med:
                return Color.yellow;
            case Tier.Low:
                // orange
                return new Color(1.0f, 0.54f, 0.0f);
            case Tier.Panic:
            default:
                return Color.red;
        }
    }
}
