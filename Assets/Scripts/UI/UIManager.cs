using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public FormationPanel FormationPanel { get; private set; }

    public ArmyPanel ArmyPanel { get; private set; }

    public ArmyEditWindow ArmyEditWindow { get; private set; }

    public event EventHandler<IArmy> ArmyModified;

    void Start()
    {
        FormationPanel = FindObjectOfType<FormationPanel>();
        Debug.Assert(FormationPanel != null, "FormationPanel not found");
        FormationPanel.Hide();

        ArmyPanel = FindObjectOfType<ArmyPanel>();
        Debug.Assert(ArmyPanel != null, "ArmyPanel not found");
        ArmyPanel.Hide();

        ArmyEditWindow = FindObjectOfType<ArmyEditWindow>();
        Debug.Assert(ArmyEditWindow != null, "ArmyEditWindow not found");
        ArmyEditWindow.Hide();

        ArmyEditWindow.ArmyModified += HandleArmyModified;
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
        ArmyEditWindow.Show();
        ArmyPanel.Hide();
        ArmyEditWindow.SetArmy(army);
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
