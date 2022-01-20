using UnityEngine;

namespace FlavBattle.Entities
{
    public class GarrisonAction : ArmyAction
    {
        public GarrisonAction(Sprite icon)
            : base(icon)
        {
        }

        public override string TooltipMessage => "Enter Garrison";

        public override bool IsAvailable()
        {
            return Army.IsOnGarrison;
        }

        public override bool IsLocked()
        {
            return false;
        }

        public override void Perform()
        {

            var armyManager = Instances.Current?.Managers?.ArmyManager;
            if (armyManager != null)
            {
                armyManager.GarrisonArmy(Army);
            }
        }
    }
}
