using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.State.NodeGraph.Nodes
{
    [CreateNodeMenu("GameEvent/Input/Army")]

    public class GameEventInputArmyNode : GameEventInputGameObjectNodeBase
    {
        protected override string NodeName => "Input (Army)";

        public enum SourceType
        {
            Army,
            Spawner,
            Search
        }

        [SerializeField]
        private SourceType _sourceType = SourceType.Army;

        [AllowNesting]
        [ShowIf("IsArmySourceType")]
        public Army Army;

        [AllowNesting]
        [ShowIf("IsSpawnerSourceType")]
        public ArmyMapSpawn ArmyMapSpawn;

        private bool IsArmySourceType() { return _sourceType == SourceType.Army; }
        private bool IsSpawnerSourceType() { return _sourceType == SourceType.Spawner; }
        private bool IsSearchSourceType() { return _sourceType == SourceType.Search; }

        public override GameObject GetValue()
        {
            switch (_sourceType)
            {
                case SourceType.Army:
                    return Army?.gameObject;
                case SourceType.Spawner:
                    return ArmyMapSpawn?.SpawnedArmy?.gameObject;
                case SourceType.Search:
                default:
                    return null;
            }
        }
    }
}
