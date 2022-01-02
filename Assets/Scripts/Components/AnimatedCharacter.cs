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

namespace FlavBattle.Components
{
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
        
        public void PlayAnimation(UnitAnimatorTrigger trigger)
        {
            switch (trigger)
            {
                case UnitAnimatorTrigger.None:
                case UnitAnimatorTrigger.ShieldBlock:
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
            }
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

        public void SetAnimations(Sprite[] animations)
        {
            throw new NotImplementedException();
        }

        public void SetColor(Color color)
        {
            throw new NotImplementedException();
        }

        public void SetFlipped(bool flipped)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void ToggleSpriteVisible(bool visible)
        {
            throw new NotImplementedException();
        }
    }
}
