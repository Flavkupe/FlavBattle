using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public Unit Unit { get; private set; }

    public FloatingText DamageTextTemplate;

    private HealthBar _healthBar;
    private bool _facingLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        _healthBar = GetComponentInChildren<HealthBar>();
    }

    public void SetUnit(Unit unit, bool facingLeft)
    {
        this.Unit = unit;
        var renderer = this.GetComponent<SpriteRenderer>();
        renderer.sprite = unit.Data.Sprite;
        _facingLeft = facingLeft;
        if (facingLeft)
        {
            this.transform.rotation = Quaternion.Euler(0, 180.0f, 0);
        }
    }

    public Coroutine TakeDamage(int damage)
    {
        return StartCoroutine(TakeDamageInternal(damage));
    }

    public Coroutine AnimateDamaged()
    {
        var renderer = this.GetComponent<SpriteRenderer>();
        return StartCoroutine(renderer.FlashColor(Color.red));
    }

    public Coroutine AnimateAttack()
    {
        return StartCoroutine(AnimateAttackInternal());
    }

    public Coroutine AnimateDeath()
    {
        return StartCoroutine(AnimateDeathInternal());
    }

    private IEnumerator TakeDamageInternal(int damage)
    {
        var info = Unit.Info;

        this.CreateDamageTextOverHead(damage.ToString());

        info.CurrentStats.HP -= damage;
        if (info.CurrentStats.HP < 0)
        {
            Debug.Log($"{Unit.Info.Name} of {Unit.Info.Faction} has died!");
            yield break;
        }
        else
        {
            var percent = (float)info.CurrentStats.HP / (float)info.MaxStats.HP;
            percent = Mathf.Clamp(percent, 0.0f, 1.0f);
            _healthBar.SetPercent(percent);
        }

        yield return AnimateDamaged();
    }

    private IEnumerator AnimateAttackInternal()
    {
        var startPos = transform.position;
        var direction = _facingLeft ? Vector3.left : Vector3.right;
        direction *= 0.25f;
        yield return this.MoveBy(direction, 5.0f);
        yield return this.MoveBy(-direction, 5.0f);
        transform.position = startPos;
    }

    private IEnumerator AnimateDeathInternal()
    {
        var renderer = this.GetComponent<SpriteRenderer>();
        yield return renderer.FadeAway(2.0f);
    }


    private void CreateDamageTextOverHead(string text)
    {
        var currentY = this.transform.position.y;
        var damageNumber = Instantiate(DamageTextTemplate);
        damageNumber.SetText(text);
        damageNumber.transform.position = this.transform.position.SetY(currentY + 0.25f);
    }

    
}
