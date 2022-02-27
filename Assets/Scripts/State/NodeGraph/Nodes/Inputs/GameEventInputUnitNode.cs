using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.State.NodeGraph.Nodes
{
    [CreateNodeMenu("GameEvent/Input/Unit")]

    public class GameEventInputUnitNode : GameEventInputGameObjectNodeBase
    {
        protected override string NodeName => "Input (Unit)";

        public Army Army;

        public override GameObject GetValue()
        {
            return Army.gameObject;
        }
    }
}
