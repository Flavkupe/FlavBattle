using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyMapSpawn : MonoBehaviour
{
    [AssetIcon]
    public Sprite Icon => Formation?.Icon;

    [Required]
    public Army ArmyTemplate;

    public FactionData Faction;

    public bool DestroyOnSpawn = true;

    [Required]
    public FormationData Formation;

    public Army SpawnArmy()
    {
        var formation = Formation.CreateFormation(Faction.Faction);
        var army = Army.CreateFromFormation(ArmyTemplate, formation);
        army.SetFaction(Faction);
        army.transform.position = this.transform.position;
        return army;
    }
}
