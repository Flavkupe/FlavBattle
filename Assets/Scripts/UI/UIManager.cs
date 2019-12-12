using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public FormationPanel FormationPanel { get; private set; }

    public ArmyPanel ArmyPanel { get; private set; }

    void Start()
    {
        FormationPanel = FindObjectOfType<FormationPanel>();
        Debug.Assert(FormationPanel != null, "FormationPanel not found");
        FormationPanel.Hide();

        ArmyPanel = FindObjectOfType<ArmyPanel>();
        Debug.Assert(ArmyPanel != null, "ArmyPanel not found");
        ArmyPanel.Hide();
    }

    public void ToggleArmyPanel()
    {
        ArmyPanel.ToggleActive();
    }
}
