using FlavBattle.Components;
using FlavBattle.Entities;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlavBattle.Formation
{
    [RequireComponent(typeof(WithFormation))]
    public class ArmyMapViewUnit : MonoBehaviour, IAnimatedSprite
    {
        [SerializeField]
        [Required]
        private WithFormation _formation;

        [SerializeField]
        [Required]
        private StatOverlay _overlay;

        [SerializeField]
        private AnimatedCharacterVisuals _characterVisuals;


        private Unit _unit;

        private AnimatedCharacter _character;


        public bool IsEmpty => _unit == null;

        public FormationPair Pair => _formation.Pair;

        public bool Matches(FormationPair pair) => _formation.Matches(pair);

        /// <summary>
        /// Creates and returns an instantiated unit.
        /// </summary>
        public IAnimatedSprite CreateUnit(Unit unit)
        {
            _unit = unit;
            var instance = Instantiate(unit.Data.AnimatedCharacter, this.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.SetVisuals(_characterVisuals);
            _character = instance;
            return instance;
        }

        public void Clear()
        {
            _unit = null;
            if (_character != null)
            {
                Destroy(_character.gameObject);
                _character = null;
            }
        }

        /// <summary>
        /// Updates with info from entire Army.
        /// </summary>
        /// <param name="summary">Data summar about whole army this is part of.</param>
        public void UpdateArmyOverlay(UnitStatSummary summary)
        {
            var hp = _unit.HPRatio;
            var morale = _unit.Info.Morale;

            var ownSummary = this._unit.GetStatSummary(false);
            ownSummary.Apply(summary);

            _overlay.UpdateOverlay(ownSummary, morale, hp);
        }

        public void SetOverlayVisible(bool visible)
        {
            this._overlay.SetActive(visible);
        }

        public void SetColor(Color color)
        {
            if (_character != null)
            {
                this._character.SetColor(color);
            }
        }

        public void SetFlippedLeft(bool flippedLeft)
        {
            if (_character != null)
            {
                this._character.SetFlippedLeft(flippedLeft);
            }
        }

        public void SetIdle(bool idle)
        {
            if (_character != null)
            {
                this._character.SetIdle(idle);
            }
        }

        public void SetSpeedModifier(float modifier)
        {
            if (_character != null)
            {
                this._character.SetSpeedModifier(modifier);
            }
        }

        public void ToggleSpriteVisible(bool visible)
        {
            if (_character != null)
            {
                this._character.ToggleSpriteVisible(visible);
            }
        }

        public void SetSortingLayer(string layer, int value)
        {
            if (_character != null)
            {
                this._character.SetSortingLayer(layer, value);
            }
        }
    }
}
