using FlavBattle.Core;
using System.Collections;
using UnityEngine;

namespace FlavBattle.State.NodeGraph.Nodes
{
    [CreateNodeMenu("GameEvent/Single/Camera")]
    [NodeTint("#0055E6")]
    public class GameEventCameraNode : GameEventNodeSingleStepBase
    {
        protected override string NodeName => "Camera";

        [Input]
        public GameEventInputGameObjectNodeBase Target;

        public GameObject GetTarget()
        {
            return GetInputValue<GameObject>(nameof(Target));
        }

        public bool PanToTarget;

        public ZoomMode Zoom;

        public float PanSpeed = 10.0f;

        public float ZoomSpeed = 10.0f;

        public enum ZoomMode
        {
            None,
            ZoomIn,
            ZoomOut,
        }

        protected override IGameEventNodeGraphStep GetStepImpl(GameEventNodeGraphOptions options)
        {
            return new Runner(options, this);
        }

        private class Runner : GameEventNodeRunnerBase
        {
            private GameEventCameraNode _node;

            public Runner(GameEventNodeGraphOptions options, GameEventCameraNode node) : base(options)
            {
                _node = node;
            }

            public override IEnumerator Do()
            {
                var target = _node.GetTarget();
                if (target == null)
                {
                    Debug.LogError("Target for GameEventCameraNode is null!");
                    yield break;
                }

                var cam = Camera.main.GetComponent<CameraMain>();

                // TODO: make these parallel... somehow
                if (_node.PanToTarget)
                {
                    yield return cam.PanTo(target.transform.position, _node.PanSpeed);
                }

                if (_node.Zoom == ZoomMode.ZoomIn)
                {
                    yield return cam.ShiftToFormationView(_node.ZoomSpeed);
                }
                else if (_node.Zoom == ZoomMode.ZoomOut)
                {
                    yield return cam.ShiftToCombatZoom(_node.ZoomSpeed);
                }
            }

            public override IEnumerator OnSkipped()
            {
                var cam = Camera.main.GetComponent<CameraMain>();
                var target = _node.GetTarget();
                if (_node.PanToTarget && target != null)
                {
                    cam.transform.position = cam.transform.position.SetXY(target.transform.position);
                }

                yield return null;
            }
        }
    }
}
