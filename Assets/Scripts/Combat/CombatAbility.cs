using FlavBattle.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CombatAbility : MonoBehaviour
{
    private CombatAbilityData _data;

    public event EventHandler TargetHit;

    /// <summary>
    /// Happens directly before the attack animation starts (such as sword swing)
    /// but after movement to target.
    /// </summary>
    public event EventHandler AttackAnimationStarting;

    public void InitData(CombatAbilityData data)
    {
        _data = data;
    }

    public Coroutine StartTargetedAbility(GameObject source, GameObject target)
    {
        if (_data.VisualEffect == CombatAbilityVisual.Projectile)
        {
            return StartCoroutine(FireProjectile(source, target));
        }
        else if (_data.VisualEffect == CombatAbilityVisual.Animation)
        {
            var coroutine = StartCoroutine(DoAnimation(source, target));
            if (_data.WaitForCompletion)
            {
                return coroutine;
            }
        }

        return null;
    }

    public Coroutine StartUntargetedAbility(GameObject source)
    {
        return StartCoroutine(DoFullSelfCombatAnimation(source));
    }

    private IEnumerator DoFullSelfCombatAnimation(GameObject source)
    {
        yield return AnimateTarget(source);
        Destroy(this.gameObject);
    }

    private IEnumerator DoAnimation(GameObject source, GameObject target)
    {
        var sourcePos = source.transform.position;
        var targetPos = GetTargetPos(target, _data.CharacterMoveTarget, 0.5f);

        // Move there
        if (_data.CharacterMove)
        {
            if (_data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Arc)
            {
                yield return MoveInArc(sourcePos, targetPos, source, _data.CharacterMoveSpeed, _data.CharacterMoveArcHeight);
            }
            else if (_data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Straight)
            {
                // TODO
            }
            else
            {
                // Teleport
                source.transform.position = targetPos;
            }
        }

        AttackAnimationStarting?.Invoke(this, new EventArgs());

        yield return AnimatorAnimation(source);

        // Animate the character
        var animationTarget = _data.CombatAnimationTarget == CombatAnimationTarget.Self ? source : target;
        yield return AnimateTarget(animationTarget);

        TargetHit?.Invoke(this, new EventArgs());

        // Move back
        if (_data.CharacterMove)
        {
            if (_data.CharacterMoveBackEffect == CombatAbilityCharacterMoveEffect.Arc)
            {
                yield return MoveInArc(targetPos, sourcePos, source, _data.CharacterMoveSpeed, _data.CharacterMoveArcHeight);
            }
            else if (_data.CharacterMoveBackEffect == CombatAbilityCharacterMoveEffect.Straight)
            {
                // TODO
            }
            else
            {
                // Teleport
                source.transform.position = sourcePos;
            }
        }

        Destroy(this.gameObject);
    }

    private IEnumerator AnimateTarget(GameObject target)
    {
        if (_data.ComabtAnimation != null)
        {
            for (int i = 0; i < _data.CombatAnimationRepeats; i++)
            {
                var instance = Instantiate(_data.ComabtAnimation.Instance);
                var animation = instance.GetComponent<IPlayableAnimation>();
                animation.Speed *= _data.CombatAnimationSpeed;
                instance.transform.SetParent(target.transform, _data.ComabtAnimation.ScaleToTarget);
                instance.transform.position = target.transform.position;
                yield return animation.PlayToCompletion();
                Destroy(instance);
            }
        }
    }

    private IEnumerator AnimatorAnimation(GameObject source)
    {
        // Play animator animation first
        if (_data.AnimatorTrigger != UnitAnimatorTrigger.None)
        {
            var combatUnit = source.GetComponent<CombatUnit>();
            if (combatUnit == null)
            {
                Debug.LogError("No CombatUnit for animation");
                yield break;
            }
            else
            {
                yield return combatUnit.PlayAnimatorToCompletion(_data.AnimatorTrigger);
            }
        }
    }

    private IEnumerator FireProjectile(GameObject source, GameObject target)
    {
        yield return AnimatorAnimation(source);

        var targetPos = target.transform.position;
        var sourcePos = source.transform.position;

        if (_data.ProjectileObject == null)
        {
            Debug.LogError($"No projectile for ability {this._data.Name}");
        }

        var projectile = Instantiate(_data.ProjectileObject);
        projectile.transform.position = sourcePos;

        if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Straight)
        {
            yield return FireProjectileStraight(sourcePos, targetPos, projectile);
        }
        else if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Arc)
        {
            yield return FireProjectileArc(sourcePos, targetPos, projectile);
        }

        TargetHit?.Invoke(this, new EventArgs());

        Destroy(projectile.gameObject);
        Destroy(this.gameObject);
    }

    private IEnumerator FireProjectileArc(Vector3 source, Vector3 target, GameObject projectile)
    {
        var height = UnityEngine.Random.Range(_data.ArcHeight.x, _data.ArcHeight.y);
        yield return MoveInArc(source, target, projectile, _data.ProjectileSpeed, height, _data.TraceDirection);
    }

    private IEnumerator MoveInArc(Vector3 source, Vector3 target, GameObject obj, float speed, float arcHeight, bool traceDirection = false)
    {
        var dist = Vector3.Distance(target, source);
        var height = arcHeight;
        var bezier = GetArc(height, source, target);

        var travelled = 0.0f;
        var starting = obj.transform.rotation;

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

    private Bezier GetArc(float height, Vector3 source, Vector3 target)
    {
        Vector3 arcPoint = (source + target) / 2.0f;
        arcPoint += Vector3.up * height;

        var p0 = source;
        var p1 = arcPoint;
        var p2 = target;
        return new Bezier(p0, p1, p2);
    }

    private IEnumerator FireProjectileStraight(Vector3 source, Vector3 target, GameObject projectile)
    {
        var dist = Vector3.Distance(target, source);
        var travelled = 0.0f;
        while (dist > 0 && travelled < dist)
        {
            var speed = _data.ProjectileSpeed * TimeUtils.FullAdjustedGameDelta;
            travelled += speed;
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target, speed);
            yield return null;
        }
    }

    private Vector3 GetTargetPos(GameObject target, CombatAbilityCharacterMoveTarget targetPos, float distance)
    {
        if (targetPos == CombatAbilityCharacterMoveTarget.Front)
        {
            // Note: since left-facing units are flipped, this should still work since "right" is
            // facing towards *their* right.
            return target.transform.position + (target.transform.right * distance);
        }

        // TEMP
        return  target.transform.position + (target.transform.right * distance);
    }
}
