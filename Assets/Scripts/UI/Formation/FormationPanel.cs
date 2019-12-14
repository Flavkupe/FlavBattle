using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FormationPanel : MonoBehaviour
{
    public UIFormationGrid Grid;

    // Start is called before the first frame update
    void Awake()
    {
        FormationUtils.PopulateFormationGrid(Grid, FormationOrientation.BottomRight, 50.0f);
    }

    public void ClearFormation()
    {
        Grid.SetArmy(null);
    }

    public void SetArmy(Army army)
    {
        Grid.SetArmy(army);
    }
}
