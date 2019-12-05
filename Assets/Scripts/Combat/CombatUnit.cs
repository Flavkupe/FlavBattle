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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUnit(Unit unit, bool facingLeft)
    {
        this.Unit = unit;
        var renderer = this.GetComponent<SpriteRenderer>();
        renderer.sprite = unit.Data.Sprite;
        _facingLeft = facingLeft;
        if (facingLeft)
        {
            renderer.flipX = true;
        }
    }

    public Coroutine TakeDamage(int damage)
    {
        var info = Unit.Info;

        this.CreateDamageTextOverHead(damage.ToString());

        info.CurrentStats.HP -= damage;
        if (info.CurrentStats.HP < 0)
        {
            // TODO: die
            Debug.Log($"{Unit.Info.Name} of {Unit.Info.Faction} has died!");
        }
        else
        {
            var percent = (float)info.CurrentStats.HP / (float)info.MaxStats.HP;
            percent = Mathf.Clamp(percent, 0.0f, 1.0f);
            _healthBar.SetPercent(percent);
        }

        return AnimateDamaged();
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

    private IEnumerator AnimateAttackInternal()
    {
        var startPos = transform.position;
        var direction = _facingLeft ? Vector3.left : Vector3.right;
        direction *= 0.25f;
        yield return this.MoveBy(direction, 5.0f);
        yield return this.MoveBy(-direction, 5.0f);
        transform.position = startPos;
    }

    private void CreateDamageTextOverHead(string text)
    {
        var currentY = this.transform.position.y;
        var damageNumber = Instantiate(DamageTextTemplate);
        damageNumber.SetText(text);
        damageNumber.transform.position = this.transform.position.SetY(currentY + 0.25f);
    }
}
