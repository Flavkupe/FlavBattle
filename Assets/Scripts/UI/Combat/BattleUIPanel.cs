﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using TMPro;

public class BattleUIPanel : MonoBehaviour
{
    [Required]
    public ToggleableButton DefensiveButton;

    [Required]
    public ToggleableButton OffensiveButton;

    [Required]
    public ArmyMoraleBar LeftMoraleBar;

    [Required]
    public ArmyMoraleBar RightMoraleBar;

    [Required]
    public BattleCommandMenu CommandMenu;

    [Required]
    public CombatTextCallout CombatTextCallout;

    [Required]
    public CombatTextCallout InfoTextCallout;

    [Required]
    public TextMeshProUGUI BoutCounterText;

    /// <summary>
    /// Fires an event indicating that the FightingStance has been changed from the UI
    /// </summary>
    public event EventHandler<FightingStance> OnStanceChangeClicked;

    public event EventHandler<OfficerAbilityData> OnCommandAbilityUsed;

    private IArmy _playerArmy;

    // Start is called before the first frame update
    void Awake()
    {
        DefensiveButton.OnClicked += (object obj, EventArgs e) => OnStanceChangeClicked?.Invoke(this, FightingStance.Defensive);
        OffensiveButton.OnClicked += (object obj, EventArgs e) => OnStanceChangeClicked?.Invoke(this, FightingStance.Offensive);
        CommandMenu.OnAbilityClicked += HandleCommandMenuOnAbilityClicked;
    }

    /// <summary>
    /// Handles an ability clicked from the OfficerAbility command menu. Closes the menu and
    /// invokes the ability.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HandleCommandMenuOnAbilityClicked(object sender, OfficerAbilityData e)
    {
        CommandMenu.Hide();
        OnCommandAbilityUsed?.Invoke(this, e);
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

    public void UpdateMorale(IArmy left, IArmy right)
    {
        LeftMoraleBar.UpdateMorale(left.Morale);
        RightMoraleBar.UpdateMorale(right.Morale);
    }

    public void SetArmies(IArmy left, IArmy right)
    {
        LeftMoraleBar.SetArmy(left);
        RightMoraleBar.SetArmy(right);

        _playerArmy = left.Faction.IsPlayerFaction ? left : right.Faction.IsPlayerFaction ? right : null;
        if (_playerArmy != null) {
            var officer = _playerArmy.Formation.GetOfficer();
            if (officer != null)
            {
                CommandMenu.SetCommander(officer);
            }
        }
    }

    /// <summary>
    /// Shows an effect animation on morale.
    /// </summary>
    /// <param name="left">Left morale bar; otherwise is right morale bar.</param>
    /// <param name="positive">Whether change is positive or negative.</param>
    public void AnimateMoraleBar(bool left, bool positive)
    {
        var bar = left ? LeftMoraleBar : RightMoraleBar;
        bar.AnimateMoraleChange(positive);
    }

    /// <summary>
    /// Sets the data and fully animates a callout for the ability
    /// </summary>
    public IEnumerator AnimateAbilityNameCallout(CombatAbilityData ability)
    {
        this.CombatTextCallout.SetData(ability);
        yield return this.CombatTextCallout.Animate();
    }

    /// <summary>
    /// Sets the data and fully animates a callout for an officer ability
    /// </summary>
    public IEnumerator AnimateAbilityNameCallout(OfficerAbilityData ability)
    {
        this.CombatTextCallout.SetData(ability);
        yield return this.CombatTextCallout.Animate();
    }

    public IEnumerator AnimateInfoTextCallout(string text)
    {
        this.InfoTextCallout.SetText(text);
        yield return this.InfoTextCallout.Animate();
    }

    /// <summary>
    /// Sets the bout text counter to the bout number.
    /// </summary>
    public void SetBoutCounterNumber(int number)
    {
        this.BoutCounterText.text = number.ToString();
    }
}