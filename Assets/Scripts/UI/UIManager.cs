using FlavBattle.Core;
using FlavBattle.State;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public FormationPanel FormationPanel;

    public ArmyPanel ArmyPanel;
    public ActionButtonsPanel ActionButtonsPanel;
    public ArmyEditWindow ArmyEditWindow;

    public MonoBehaviour[] MapUI;

    public event EventHandler<IArmy> ArmyModified;
    public event EventHandler<Unit> UnitReplaced;
    public event EventHandler<IArmy> ArmyDeployed;

    private GameEventManager _gameEvents;

    void Start()
    {
        ArmyEditWindow.Hide();
        ArmyPanel.Hide();

        ArmyEditWindow.ArmyModified += HandleArmyModified;
        ArmyEditWindow.UnitReplaced += HandleUnitReplaced;
        ArmyEditWindow.ArmyDeployed += HandleArmyDeployed;

        _gameEvents = Instances.Current.Managers.GameEvents;
        _gameEvents.CombatStartedEvent += HandleCombatStartedEvent;
        _gameEvents.CombatEndedEvent += HandleCombatEndedEvent;

        UpdateSelectedArmyUI(null);
    }

    private void HandleArmyDeployed(object sender, IArmy army)
    {
        ArmyDeployed?.Invoke(this, army);
    }

    private void HandleCombatEndedEvent(object sender, CombatEndedEventArgs e)
    {
        ShowMapUI();
    }

    private void HandleCombatStartedEvent(object sender, CombatStartedEventArgs e)
    {
        HideMapUI();
    }

    private void HideMapUI()
    {
        FormationPanel.Hide();
        foreach (var ui in MapUI)
        {
            ui.Hide();
        }
    }

    private void ShowMapUI()
    {
        foreach (var ui in MapUI)
        {
            ui.Show();
        }
    }

    private void HandleUnitReplaced(object sender, Unit e)
    {
        UnitReplaced?.Invoke(this, e);
    }

    private void HandleArmyModified(object sender, IArmy e)
    {
        ArmyModified?.Invoke(this, e);
        ArmyPanel.UpdatePanelContents();
    }

    public void ToggleArmyPanel()
    {
        ArmyPanel.ToggleActive();
    }

    public void ShowArmyEditWindow(IArmy army)
    {
        if (!ArmyEditWindow.IsShowing())
        {
            Sounds.Play(UISoundType.Open);
            ArmyEditWindow.Show();
            ArmyPanel.Hide();
            ArmyEditWindow.SetMode(ArmyEditWindow.Mode.DeployedArmy);
            ArmyEditWindow.SetArmy(army);
        }
    }

    public void ShowGarrisonWindow(IArmy[] storedArmies, Unit[] storedUnits)
    {
        if (!ArmyEditWindow.IsShowing())
        {
            Sounds.Play(UISoundType.Open);
            ArmyEditWindow.Show();
            ArmyPanel.Hide();
            ArmyEditWindow.SetMode(ArmyEditWindow.Mode.Garrison);
            ArmyEditWindow.SetArmy(null);
            UpdateGarrisonWindow(storedArmies, storedUnits);
        }
    }

    public void UpdateGarrisonWindow(IArmy[] storedArmies, Unit[] storedUnits)
    {
        ArmyEditWindow.SetArmyPanelContents(storedArmies);
        ArmyEditWindow.SetUnitPanelContents(storedUnits);
    }


    public void HideArmyEditWindow()
    {
        Sounds.Play(UISoundType.Close);
        ArmyEditWindow.Hide();
        _gameEvents.TriggerMapEvent(MapEventType.MapUnpaused);
    }

    /// <summary>
    /// Handles all necessary events resulting from an
    /// army being created and deployed.
    /// </summary>
    public void ArmyCreated(Army army)
    {
        if (army.IsPlayerArmy)
        {
            ArmyPanel.AddArmy(army);
        }
    }

    public void UpdateSelectedArmyUI(Army selected)
    {
        if (selected == null)
        {
            this.FormationPanel.Hide();
            this.ActionButtonsPanel.SetArmy(null);
            this.ActionButtonsPanel.SetActive(false);
        }
        else
        {
            this.FormationPanel.Show();
            this.FormationPanel.SetArmy(selected);
            this.ActionButtonsPanel.SetActive(true);
            this.ActionButtonsPanel.SetArmy(selected);
        }

        this.ArmyPanel.SetSelectedArmy(selected);
    }
}
