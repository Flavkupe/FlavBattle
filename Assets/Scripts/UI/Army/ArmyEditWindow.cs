using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyEditWindow : MonoBehaviour
{
    public DropTargetUIGrid Grid;

    public void Awake()
    {
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 96.0f);
    }

    public void SetArmy(Army army)
    {
        Grid.SetArmy(army);
    }
}
