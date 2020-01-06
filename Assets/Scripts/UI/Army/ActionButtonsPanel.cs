using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtonsPanel : MonoBehaviour
{
    public GameObject GarrisonButton;

    private Army _army;

    private void Update()
    {
        if (_army != null)
        {
            GarrisonButton.SetActive(_army.IsOnGarrison);
        }
    }

    public void SetArmy(Army selected)
    {
        _army = selected;
    }
}
