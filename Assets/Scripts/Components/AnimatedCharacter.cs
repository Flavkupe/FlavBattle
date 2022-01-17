using FlavBattle.Combat;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlavBattle.Components
{
    [Serializable]
    public class AnimatedCharacterVisuals
    {
        public Vector3 StartingPosition;
        public Vector3 StartingScale;
        public SortingLayerValues SortingLayer;
    }

    /// <summary>
    /// Used for a fully animated character prefab that can be
    /// used in combat or the map, as well as any other place.
    /// </summary>
    public class AnimatedCharacter : MonoBehaviour, IAnimatedSprite
    {
        /// <summary>
        /// Animator for AnimatedCharacter animations.
        /// </summary>
        public Animator Animator => _animator;
        [Required]
        [SerializeField]
        private Animator _animator;

        [Required]
        [SerializeField]
        private SPUM_Prefabs _prefab;

        /// <summary>
        /// Fires events as animations start and complete
        /// </summary>
        public AnimationEventDispatcher AnimationEventDispatcher => _animationEventDispatcher;
        [Required]
        [SerializeField]
        private AnimationEventDispatcher _animationEventDispatcher;

        /// <summary>
        /// Uses the Animator Tag to check if the state is still the same.
        /// </summary>
        public bool IsInState(UnitAnimatorState state)
        {
            return this._prefab.IsInState(state.ToString());
        }

        public void PlayAnimation(UnitAnimatorTrigger trigger)
        {
            switch (trigger)
            {
                case UnitAnimatorTrigger.Idle:
                    this._prefab.PlayAnimation(0);
                    break;
                case UnitAnimatorTrigger.ShieldBlock:
                    this._prefab.PlayAnimation(10);
                    break;
                case UnitAnimatorTrigger.Die:
                    this._prefab.PlayAnimation(2);
                    break;
                case UnitAnimatorTrigger.Melee:
                    this._prefab.PlayAnimation(4);
                    break;
                case UnitAnimatorTrigger.ShootBow:
                    this._prefab.PlayAnimation(5);
                    break;
                case UnitAnimatorTrigger.Static:
                    this._prefab.PlayAnimation(11);
                    break;
                case UnitAnimatorTrigger.SpecialJump:
                    this._prefab.PlayAnimation(9);
                    break;
            }
        }

        public void SetFlippedLeft(bool flippedLeft)
        {
            // The SPUM animated chars face left by default
            if (flippedLeft)
            {
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                this.transform.rotation = Quaternion.Euler(0, 180.0f, 0);
            }
        }

        public void SetIdle(bool idle)
        {
            if (idle)
            {
                this._prefab.PlayAnimation(0);
            }
            else
            {
                this._prefab.PlayAnimation(1);
            }
        }

        public void SetSpeedModifier(float modifier)
        {
            // TODO
        }

        public void SetColor(Color color)
        {
            var children = _prefab.GetComponentsInChildren<SpriteRenderer>();
            foreach(var child in children)
            {
                child.color = color;
            }
        }

        public void ToggleSpriteVisible(bool visible)
        {
            _prefab.gameObject.SetActive(visible);
        }

        [ContextMenu("Preview to Texture")]
        private void PreviewToTexture()
        {
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
            var name = Selection.activeObject.name;
            var texture = AssetPreview.GetAssetPreview(this.gameObject);
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(path, name + ".png"), bytes);
        }

        public void SetSortingLayer(string layer, int value)
        {
            var sortingGroup = GetComponentInChildren<SortingGroup>();
            sortingGroup.sortingLayerName = layer;
            sortingGroup.sortingOrder = value;
        }

        public void SetVisuals(AnimatedCharacterVisuals visuals)
        {
            if (!visuals.StartingPosition.Equals(Vector3.zero))
            {
                this.transform.localPosition = visuals.StartingPosition;
            }

            if (!visuals.StartingScale.Equals(Vector3.zero))
            {
                this.transform.localScale = visuals.StartingScale;
            }

            this.SetSortingLayer(visuals.SortingLayer.Name, visuals.SortingLayer.Value);
        }
    }
}
