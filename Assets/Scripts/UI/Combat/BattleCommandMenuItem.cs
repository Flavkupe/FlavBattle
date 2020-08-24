using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCommandMenuItem : MonoBehaviour
{
    [Required]
    public VisualCounter Cost;

    [Required]
    public Image Icon;

    [Required]
    public TextMeshProUGUI Text;


    public void SetAbility(OfficerAbilityData data)
    {
        Cost.SetCount(data.CommandCost);
        Text.text = data.Name;
        Icon.sprite = data.Icon;
    }
}
