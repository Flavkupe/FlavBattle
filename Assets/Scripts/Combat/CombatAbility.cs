using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CombatAbility : MonoBehaviour
{
    private CombatAbilityData _data;

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

        return null;
    }

    private IEnumerator FireProjectile(GameObject source, GameObject target)
    {
        var targetPos = target.transform.position;
        var sourcePos = source.transform.position;
        var projectile = Instantiate(_data.ProjectileObject);
        projectile.transform.position = sourcePos;
        var dist = Vector3.Distance(targetPos, sourcePos);

        if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Straight)
        {
            yield return FireProjectileStraight(sourcePos, targetPos, projectile, dist);
        }
        else if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Arc)
        {
            yield return FireProjectileArc(sourcePos, targetPos, projectile, dist);
        }

        Destroy(projectile.gameObject);
        Destroy(this.gameObject);
    }

    private IEnumerator FireProjectileArc(Vector3 source, Vector3 target, GameObject projectile, float dist)
    {
        
        var height = UnityEngine.Random.Range(_data.ArcHeight.x, _data.ArcHeight.y);
        Vector3 arcPoint = (source + target) / 2.0f;
        arcPoint += Vector3.up * height;

        var p0 = source;
        var p1 = arcPoint;
        var p2 = target;
        var bezier = new Bezier(p0, p1, p2);

        var travelled = 0.0f;
        var starting = projectile.transform.rotation;
        while (dist > 0 && travelled < dist)
        {
            var speed = _data.ProjectileSpeed * Time.deltaTime;
            travelled += speed;

            var t = travelled / dist;
            projectile.transform.position = bezier.GetPoint(t);

            if (_data.TraceDirection)
            {
                // var direction = bezier.GetDirection(projectile.transform, t);
                var direction = bezier.GetDirection(t);
                // var direction = bezier.GetFirstDerivative(t);
                Debug.Log(direction);
                projectile.transform.rotation = Quaternion.LookRotation(direction, Vector3.forward);
            }

            yield return null;
        }
    }

    private IEnumerator FireProjectileStraight(Vector3 source, Vector3 target, GameObject projectile, float dist)
    {
        var travelled = 0.0f;
        while (dist > 0 && travelled < dist)
        {
            var speed = _data.ProjectileSpeed * Time.deltaTime;
            travelled += speed;
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, target, speed);
            yield return null;
        }
    }
}
