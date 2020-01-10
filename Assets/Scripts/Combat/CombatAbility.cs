using System;
using System.Collections;
using System.Collections.Generic;
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
        var travelled = 0.0f;

        Vector3? arcPoint = null; 
        if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Arc)
        {
            var height = UnityEngine.Random.Range(_data.ArcHeight.x, _data.ArcHeight.y);
            arcPoint = (targetPos + sourcePos) / 2.0f;
            arcPoint += Vector3.up * height;
        }

        while (dist > 0 && travelled < dist)
        {
            var speed = _data.ProjectileSpeed * Time.deltaTime;
            travelled += speed;

            if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Straight)
            {
                projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPos, speed);
            }
            else if (_data.ProjectileEffect == CombatAbilityProjectileEffect.Arc)
            {
                var p0 = sourcePos;
                var p1 = arcPoint.Value;
                var p2 = targetPos;
                var t = travelled / dist;
                projectile.transform.position = Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
            }

            yield return null;
        }

        Destroy(projectile.gameObject);
        Destroy(this.gameObject);
    }
}
