using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BattleCommandMenuItem : MonoBehaviour
{
    private OfficerAbilityData ability;

    [Required]
    public VisualCounter Cost;

    [Required]
    public Image Icon;

    [Required]
    public TextMeshProUGUI Text;

    public event EventHandler<OfficerAbilityData> OnClicked;

    public void SetAbility(OfficerAbilityData data)
    {
        Cost.SetCount(data.CommandCost);
        Text.text = data.Name;
        Icon.sprite = data.Icon;
        this.ability = data;
    }

    public void UpdateState(int commandPointsAvailable)
    {
        this.GetComponent<Button>().interactable = commandPointsAvailable >= this.ability.CommandCost;
    }

    public void ItemClicked()
    {
        if (this.ability != null)
        {
            OnClicked?.Invoke(this, this.ability);
        }
    }
}
