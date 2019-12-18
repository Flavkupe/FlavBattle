using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarrisonManager : MonoBehaviour
{
    public bool IsCombatMap = true;

    private List<Army> _garrisonedArmies = new List<Army>();

    private ArmyManager _armyManager;
    private void Awake()
    {
        if (IsCombatMap)
        {
            // can deploy armies and such
            _armyManager = FindObjectOfType<ArmyManager>();
        }

        // TEMP
        
    }
}
