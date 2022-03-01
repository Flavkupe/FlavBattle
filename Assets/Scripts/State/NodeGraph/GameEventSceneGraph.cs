using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XNode;

namespace FlavBattle.State.NodeGraph
{
    public class GameEventSceneGraph : SceneGraph<GameEventNodeGraph>
    {
        [ContextMenu("Start Events")]
        public void StartEvents()
        {
            var input = new GaveEventNodeGraphInput()
            {
                Initiator = this,
            };

            var step = this.graph.GetStartStep(input);
            StartCoroutine(step.Do());
        }
    }
}
