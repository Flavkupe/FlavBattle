using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public FormationPanel FormationPanel { get; private set; }

    public ArmyPanel ArmyPanel;

    public MonoBehaviour[] MapUI;

    public ArmyEditWindow ArmyEditWindow { get; private set; }

    public event EventHandler<IArmy> ArmyModified;

    public event EventHandler<Unit> UnitReplaced;

    private GameEventManager _gameEvents;

    void Start()
    {
        FormationPanel = FindObjectOfType<FormationPanel>();
        Debug.Assert(FormationPanel != null, "FormationPanel not found");
        FormationPanel.Hide();

        ArmyEditWindow = FindObjectOfType<ArmyEditWindow>();
        Debug.Assert(ArmyEditWindow != null, "ArmyEditWindow not found");
        ArmyEditWindow.Hide();

        ArmyEditWindow.ArmyModified += HandleArmyModified;
        ArmyEditWindow.UnitReplaced += HandleUnitReplaced;

        _gameEvents = FindObjectOfType<GameEventManager>();
        _gameEvents.CombatStartedEvent += HandleCombatStartedEvent;
        _gameEvents.CombatEndedEvent += HandleCombatEndedEvent;
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
            ArmyEditWindow.Show();
            ArmyPanel.Hide();
            ArmyEditWindow.SetMode(ArmyEditWindow.Mode.Garrison);
            ArmyEditWindow.SetArmy(null);
            ArmyEditWindow.SetArmyPanelContents(storedArmies);
            ArmyEditWindow.SetUnitPanelContents(storedUnits);
        }
    }

    public void HideArmyEditWindow()
    {
        ArmyEditWindow.Hide();
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
}
