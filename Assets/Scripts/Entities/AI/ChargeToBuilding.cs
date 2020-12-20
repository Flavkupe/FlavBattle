using System;
using UnityEngine;

public class ChargeToBuilding : ArmyMapAIBase
{
    public GameObject Target;

    private TravelPath _path;

    // TODO: is there a better way...?
    private bool _continue = false;

    private void Start()
    {
        if (Target.GetComponent<IDetectable>() == null)
        {
            Debug.LogWarning($"Target for ${this.name} is not IDetectable");
        }
    }

    public override bool ShouldContinueAction => _continue;

    public override void DoActionTick(Army army, TilemapManager map)
    {
        if (Target == null)
        {
            // No target
            return;
        }

        _continue = true;

        if (army.HasDestination)
        {
            // already charging
            return;
        }

        var detectable = Target.GetComponent<IDetectable>();
        if (army.Detects(detectable))
        {
            // Already in position. Reset path just in case
            _path = null;
            return;
        }

        if (_path == null)
        {
            // The only condition to stop this action is if the
            // building is no longer found and there is no path.
            _continue = false;
            return;
        }
            
        army.SetPath(_path);
    }

    public override bool IsActionPossible(Army army, TilemapManager map)
    {
        Debug.Log("Checking if action is possible");
        if (Target == null)
        {
            // No target
            return false;
        }

        var startTile = map.GetGridTileAtWorldPos(army.gameObject);
        var endTile = map.GetGridTileAtWorldPos(Target);
        _path = map.GetPath(startTile, endTile);

        // Possible if a path exists
        return _path != null;
    }
}
