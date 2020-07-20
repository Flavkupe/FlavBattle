using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyMoraleBar : MonoBehaviour
{
    [Required]
    public MoraleIcon MoraleIcon;

    [Required]
    public Image MoraleBar;

    [Required]
    public Image ArmyPortrait;

    public void UpdateMorale(Morale morale)
    {
        var color = morale.GetColor();
        MoraleBar.SetColor(color);
        MoraleBar.fillAmount = (float)morale.Current / 100.0f;
        MoraleIcon.UpdateIcon(morale);
    }

    public void AnimateMoraleChange(bool positive)
    {
        if (positive)
        {
            MoraleIcon.AnimatePositiveChange();
        }
        else
        {
            MoraleIcon.AnimateNegativeChange();
        }
    }

    public void SetArmy(IArmy army)
    {
        UpdateMorale(army.Morale);
        ArmyPortrait.sprite = army.Formation.GetOfficer().Info.Portrait;
    }
}
