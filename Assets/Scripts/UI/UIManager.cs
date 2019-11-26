using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private FormationPanel _formationPanel;
    public FormationPanel FormationPanel => _formationPanel;

    void Start()
    {
        _formationPanel = FindObjectOfType<FormationPanel>();
        Debug.Assert(_formationPanel != null, "FormationPanel not found");
        _formationPanel.Hide();
    }
}
