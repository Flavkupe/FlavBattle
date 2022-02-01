using System.Collections;
using UnityEngine;

namespace FlavBattle.Combat.Animation
{
    public static class AnimationUtils
    {
        public static IEnumerator MoveInArc(Vector3 source, Vector3 target, GameObject obj, float speed, float arcHeight, bool traceDirection = false)
        {
            var dist = Vector3.Distance(target, source);
            var height = arcHeight;
            var bezier = GetArc(height, source, target);

            var travelled = 0.0f;

            while (dist > 0 && travelled < dist)
            {
                var step = speed * TimeUtils.FullAdjustedGameDelta;
                travelled += step;

                var t = travelled / dist;
                obj.transform.position = bezier.GetPoint(t);

                if (traceDirection)
                {
                    var direction = bezier.GetDirection(t);
                    var targetPt = obj.transform.position + direction;

                    obj.transform.LookAt(targetPt, Vector3.up);
                    // WHYYYYYYY????? This seems to be required for this to work
                    obj.transform.Rotate(0, 90, 180);
                }

                yield return null;
            }
        }

        public static Bezier GetArc(float height, Vector3 source, Vector3 target)
        {
            Vector3 arcPoint = (source + target) / 2.0f;
            arcPoint += Vector3.up * height;

            var p0 = source;
            var p1 = arcPoint;
            var p2 = target;
            return new Bezier(p0, p1, p2);
        }

        /// <summary>
        /// Finds position relative to target
        /// </summary>
        public static Vector3 GetTargetPos(CombatUnit target, CombatAbilityCharacterMoveTarget targetPos, float distance)
        {
            var jitterMin = -0.1f;
            var jitterMax = 0.1f;
            var jitter = new Vector3(
                UnityEngine.Random.Range(jitterMin, jitterMax),
                UnityEngine.Random.Range(jitterMin, jitterMax),
                0);
            if (targetPos == CombatAbilityCharacterMoveTarget.Front)
            {
                return target.Front.position + jitter;
            }

            // TEMP
            return target.Back.position + jitter;
        }
    }
}
