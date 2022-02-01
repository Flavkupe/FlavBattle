using FlavBattle.Combat;
using FlavBattle.Combat.Animation;
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

    /// <summary>
    /// Fires when the animator finishes its effect (such as drawing a bow)
    /// </summary>
    public event EventHandler AttackAnimatorCompleted;

    public void InitData(CombatAbilityData data)
    {
        _data = data;
    }

    public Coroutine StartTargetedAbility(CombatUnit source, CombatUnit target)
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

    public Coroutine StartUntargetedAbility(CombatUnit source)
    {
        return StartCoroutine(DoFullSelfCombatAnimation(source));
    }

    private IEnumerator DoFullSelfCombatAnimation(CombatUnit source)
    {
        yield return AnimateTarget(source.transform);
        Destroy(this.gameObject);
    }

    private IEnumerator DoAnimation(CombatUnit source, CombatUnit target)
    {
        var character = source.Character;
        var sourcePos = character.transform.position;
        var targetPos = GetTargetPos(target, _data.CharacterMoveTarget, 0.5f);

        // Move there
        if (_data.CharacterMove)
        {
            if (_data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Arc)
            {
                yield return AnimationUtils.MoveInArc(sourcePos, targetPos, character.gameObject, _data.CharacterMoveSpeed, _data.CharacterMoveArcHeight);
            }
            else if (_data.CharacterMoveToEffect == CombatAbilityCharacterMoveEffect.Straight)
            {
                // TODO
            }
            else
            {
                // Teleport
                character.transform.position = targetPos;
            }
        }

        AttackAnimationStarting?.Invoke(this, new EventArgs());

        yield return AnimatorAnimation(source);

        // Animate the character
        var animationTarget = _data.CombatAnimationTarget == CombatAnimationTarget.Self ? character.transform : target.transform;
        yield return AnimateTarget(animationTarget);

        TargetHit?.Invoke(this, new EventArgs());

        // Move back
        if (_data.CharacterMove)
        {
            if (_data.CharacterMoveBackEffect == CombatAbilityCharacterMoveEffect.Arc)
            {
                yield return AnimationUtils.MoveInArc(targetPos, sourcePos, character.gameObject, _data.CharacterMoveSpeed, _data.CharacterMoveArcHeight);
            }
            else if (_data.CharacterMoveBackEffect == CombatAbilityCharacterMoveEffect.Straight)
            {
                // TODO
            }
            else
            {
                // Teleport
                character.transform.position = sourcePos;
            }
        }

        Destroy(this.gameObject);
    }

    private IEnumerator AnimateTarget(Transform target)
    {
        if (_data.ComabtAnimation != null)
        {
            for (int i = 0; i < _data.CombatAnimationRepeats; i++)
            {
                var instance = Instantiate(_data.ComabtAnimation.Instance);
                var animation = instance.GetComponent<IPlayableAnimation>();
                animation.Speed *= _data.CombatAnimationSpeed;
                instance.transform.SetParent(target, _data.ComabtAnimation.ScaleToTarget);
                instance.transform.position = target.position;
                yield return animation.PlayToCompletion();
                Destroy(instance.gameObject);
            }
        }
    }

    private IEnumerator AnimatorAnimation(CombatUnit source)
    {
        // Play animator animation first
        if (_data.AnimatorTrigger != UnitAnimatorTrigger.Idle)
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
                AttackAnimatorCompleted?.Invoke(this, new EventArgs());
            }
        }
    }

    private IEnumerator FireProjectile(CombatUnit source, CombatUnit target)
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
        yield return AnimationUtils.MoveInArc(source, target, projectile, _data.ProjectileSpeed, height, _data.TraceDirection);
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

    /// <summary>
    /// Finds position relative to target
    /// </summary>
    private Vector3 GetTargetPos(CombatUnit target, CombatAbilityCharacterMoveTarget targetPos, float distance)
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
        return  target.Back.position + jitter;
    }
}
