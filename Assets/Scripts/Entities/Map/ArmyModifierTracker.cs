using FlavBattle.Entities.Modifiers;
using FlavBattle.Modifiers;
using NaughtyAttributes;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FlavBattle.Entities
{
    /// <summary>
    /// Tracks and updates stats and modifiers for Army.
    /// </summary>
    public class ArmyModifierTracker : MonoBehaviour
    {
        private ThrottleTimer _throttle = new ThrottleTimer(1.0f);

        private ArmyModifierSet _modifiers = new ArmyModifierSet();

        [SerializeField]
        [Required]
        private ArmyMapView _mapView;

        [SerializeField]
        [Required]
        private Army _army;



        private void Awake()
        {
            // TODO: better way to set these?
            if (_modifiers.GetModifiers().Count() == 0)
            {
                _modifiers.AddModifier(new RelativeArmyModifier());
                _modifiers.AddModifier(new ArmyMoraleModifier());
                _modifiers.AddModifier(new MapTileModifier());
            }
        }

        void Update()
        {
            if (_throttle.Tick(TimeUtils.AdjustedGameDelta))
            {
                this.UpdateModifiers();
                this.UpdateOverlay();
            }
        }

        /// <summary>
        /// Updates the tally of modifiers. Should run every few update frames.
        /// </summary>
        private void UpdateModifiers()
        {
            _modifiers.UpdateModifiers(_army);
        }

        /// <summary>
        /// Updates the overlay for the MapView
        /// </summary>
        private void UpdateOverlay()
        {
            if (_mapView != null)
            {
                this._mapView.UpdateArmyOverlay();
            }
        }

        /// <summary>
        /// Gets the modifiers for Army itself.
        /// </summary>
        public ModifierSet GetModifiers()
        {
            UpdateModifiers();
            return _modifiers;
        }
    }
}
