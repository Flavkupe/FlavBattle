using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using TMPro;
using FlavBattle.UI.Combat;

public class BattleUIPanel : MonoBehaviour
{


    [Required]
    [SerializeField]
    private CombatStancePanel StancePanel;

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

    [Required]
    public AttackStats AttackStats;

    /// <summary>
    /// Fires an event indicating that the FightingStance has been changed from the UI
    /// </summary>
    public event EventHandler<FightingStance> OnStanceChangeClicked;

    public event EventHandler<OfficerAbilityData> OnCommandAbilityUsed;

    private IArmy _playerArmy;

    // Start is called before the first frame update
    void Awake()
    {
        StancePanel.OnStanceChangeClicked += HandleStanceClicked;
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

    /// <summary>
    /// Shows the stance panel to select stance
    /// </summary>
    public void ShowStancePanel()
    {
        StancePanel.Show();
    }

    /// <summary>
    /// Handles selection of a stance from the UI
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    public void HandleStanceClicked(object source, FightingStance e)
    {
        StancePanel.Hide();
        OnStanceChangeClicked?.Invoke(source, e);
    }
}
