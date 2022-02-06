using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Custom/Abilities/Animation/Projectile", order = 1)]
    public class AnimationProjectileData : CombatAnimationData
    {
        [Tooltip("Arc, straight line, etc")]
        public CombatAbilityProjectileEffect Trajectory;

        [Tooltip("Object that will be instantiated for projectile.")]
        [Required]
        public GameObject Projectile;

        [Tooltip("Projectile travel velocity.")]
        public float Speed = 10.0f;

        [Tooltip("Whether to point in the direction of the trajectory")]
        [ShowIf("ShowProjectileArcProps")]
        public bool TraceDirection = true;

        [ShowIf("ShowProjectileArcProps")]
        [MinMaxSlider(-10.0f, 10.0f)]
        public Vector2 ArcHeight = new Vector2(0.0f, 0.0f);

        private bool ShowProjectileArcProps()
        {
            return Trajectory == CombatAbilityProjectileEffect.Arc;
        }

        public override ICombatAnimationStep Create(CombatAnimationOptions options)
        {
            return new AnimationProjectile(this, options);
        }
    }

    public class AnimationProjectile : CombatAnimationActionStepBase<AnimationProjectileData>
    {
        public AnimationProjectile(AnimationProjectileData data, CombatAnimationOptions options) : base(data, options)
        {
        }

        protected override IEnumerator DoAction()
        {
            if (Data.Projectile == null)
            {
                Debug.LogError($"No projectile for ability");
                yield break;
            }

            var source = ActionSummary.Source;
            var target = ActionSummary.Target;
            var sourceUnit = source.CombatUnit;
            var sourcePos = sourceUnit.transform.position;
            var targetPos = target.CombatUnit.transform.position;
            

            var projectile = GameObject.Instantiate(Data.Projectile);
            projectile.transform.position = source.CombatUnit.transform.position;

            if (Data.Trajectory == CombatAbilityProjectileEffect.Straight)
            {
                yield return FireProjectileStraight(sourcePos, targetPos, projectile);
            }
            else
            {
                yield return FireProjectileArc(sourcePos, targetPos, projectile);
            }

            GameObject.Destroy(projectile.gameObject);
        }

        private IEnumerator FireProjectileArc(Vector3 source, Vector3 target, GameObject projectile)
        {
            var height = UnityEngine.Random.Range(Data.ArcHeight.x, Data.ArcHeight.y);
            yield return AnimationUtils.MoveInArc(source, target, projectile, Data.Speed, height, Data.TraceDirection);
        }

        private IEnumerator FireProjectileStraight(Vector3 source, Vector3 target, GameObject projectile)
        {
            var dist = Vector3.Distance(target, source);
            var travelled = 0.0f;
            while (dist > 0 && travelled < dist)
            {
                var speed = Data.Speed * TimeUtils.FullAdjustedGameDelta;
                travelled += speed;
                projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target, speed);
                yield return null;
            }
        }
    }
}
