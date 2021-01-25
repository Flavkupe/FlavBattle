using System;
using UnityEngine;

public class ChargeToBuilding : ArmyMapAIBase
{
    public GameObject Target;

    private TravelPath _path;

    private void Start()
    {
        if (Target.GetComponent<IDetectable>() == null)
        {
            Debug.LogWarning($"Target for ${this.name} is not IDetectable");
        }
    }

    public override void DoAction(Army army, TilemapManager map)
    {
        if (Target == null)
        {
            // No target
            return;
        }

        if (HasReachedTarget(army))
        {
            // Already in position. Reset path just in case
            _path = null;
            army.SetPath(null);
            return;
        }
            
        army.SetPath(_path);
    }

    private bool HasReachedTarget(Army army)
    {
        var detectable = Target.GetComponent<IDetectable>();
        return army.Detects(detectable);
    }


    public override bool IsActionPossible(Army army, TilemapManager map)
    {
        Debug.Log("Checking if action is possible");
        if (Target == null)
        {
            // No target
            return false;
        }

        if (HasReachedTarget(army))
        {
            // Already at target; action not possible
            return false;
        }

        var startTile = map.GetGridTileAtWorldPos(army.gameObject);
        var endTile = map.GetGridTileAtWorldPos(Target);
        _path = map.GetPath(startTile, endTile);

        // Possible if a path exists
        return _path != null;
    }
}
