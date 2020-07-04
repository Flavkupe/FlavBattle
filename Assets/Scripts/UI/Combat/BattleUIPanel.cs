﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class BattleUIPanel : MonoBehaviour
{
    [Required]
    public ToggleableButton DefensiveButton;

    [Required]
    public ToggleableButton OffensiveButton;

    /// <summary>
    /// Fires an event indicating that the FightingStance has been changed from the UI
    /// </summary>
    public event EventHandler<FightingStance> OnStanceChangeClicked;

    // Start is called before the first frame update
    void Awake()
    {
        DefensiveButton.OnClicked += (object obj, EventArgs e) => OnStanceChangeClicked?.Invoke(this, FightingStance.Defensive);
        OffensiveButton.OnClicked += (object obj, EventArgs e) => OnStanceChangeClicked?.Invoke(this, FightingStance.Offensive);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Updates the selected button on the UI and enforces an optional
    /// cooldown (in seconds) on the buttons. If cooldown is 0, no cooldown is applied.
    /// </summary>
    /// <param name="stance">Stance to set to.</param>
    /// <param name="cooldown">Cooldown before buttons are enabled again. Use 0 for no cooldown</param>
    public void UpdateStance(FightingStance stance, float cooldownSeconds)
    {
        if (stance == FightingStance.Defensive)
        {
            DefensiveButton.ToggleSelected(true);
            OffensiveButton.ToggleSelected(false);
        }
        else
        {
            DefensiveButton.ToggleSelected(false);
            OffensiveButton.ToggleSelected(true);
        }

        if (cooldownSeconds > 0.0f)
        {
            DefensiveButton.Button.interactable = false;
            OffensiveButton.Button.interactable = false;
            this.DoAfter(cooldownSeconds, () => {
                DefensiveButton.Button.interactable = true;
                OffensiveButton.Button.interactable = true;
            });
        }
    }
}
