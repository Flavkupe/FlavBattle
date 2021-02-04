using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using System;

public class BattleCommandMenu : MonoBehaviour
{
    /// <summary>
    /// Area where ability point icons are shown
    /// </summary>
    [Required]
    public VisualCounter CommandPointsArea;

    /// <summary>
    /// Where the commands are listed
    /// </summary>
    [Required]
    public Transform CommandArea;

    [Required]
    public BattleCommandMenuItem CommandItemTemplate;

    public event EventHandler<OfficerAbilityData> OnAbilityClicked;

    private Unit _commander;

    private List<BattleCommandMenuItem> _items = new List<BattleCommandMenuItem>();

    public void SetOfficer(Unit officer)
    {
        _items.Clear();
        _commander = officer;
        CommandArea.DestroyChildren();

        var abilities = officer.Info.OfficerAbilities.Where(a =>
            a.TriggerType != OfficerAbilityTriggerType.Passive
        );

        foreach (var ability in abilities)
        {
            var item = Instantiate(CommandItemTemplate);
            item.SetAbility(ability);
            item.transform.SetParent(CommandArea, false);
            item.OnClicked += (o, e) => this.OnAbilityClicked?.Invoke(this, e);
            _items.Add(item);
        }

        // Update enabled state of buttons and menu
        UpdateMenu();
    }

    public void OpenMenu()
    {
        this.Show();
        this.UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (_commander == null)
        {
            return;
        }

        var commandPoints = _commander.Info.CurrentStats.Commands;
        CommandPointsArea.SetCount(commandPoints);
        foreach (var item in _items)
        {
            item.UpdateState(commandPoints);
        }
    }

    public void HandleAbilityClicked(object o, OfficerAbilityData data)
    {
        if (_commander == null)
        {
            return;
        }

        var commandPoints = _commander.Info.CurrentStats.Commands;
        if (data.CommandCost <= commandPoints)
        {
            this.OnAbilityClicked?.Invoke(this, data);
        }
    }
}
