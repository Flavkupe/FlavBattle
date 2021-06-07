using NaughtyAttributes;
using UnityEngine;

namespace FlavBattle.Entities.Data
{
    [CreateAssetMenu(fileName = "Race", menuName = "Custom/Race/Race Data", order = 1)]
    public class RaceData : ScriptableObject
    {
        [ShowAssetPreview(128, 128)]
        [AssetIcon]
        [SerializeField]
        private Sprite _icon;
        public Sprite Icon => _icon;

        [SerializeField]
        private string _name;
        public string Name => _name;

        [SerializeField]
        private string _description;
        public string Description => _description;

        [SerializeField]
        private PerkData[] _racePerks;
        public PerkData[] RacePerks => _racePerks;
    }
}
