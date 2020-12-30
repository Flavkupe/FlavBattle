using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public Unit Unit { get; private set; }

    [Required]
    public FloatingText DamageTextTemplate;

    [Required]
    public MoraleIcon MoraleIcon;

    [Required]
    public HealthBar HealthBar;

    private bool _facingLeft = false;


    // Start is called before the first frame update
    void Start()
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
            this.transform.rotation = Quaternion.Euler(0, 180.0f, 0);
        }

        this.UpdateUIComponents();
    }

    public void TakeDamage(int damage)
    {
        var info = Unit.Info;
        info.CurrentStats.HP -= damage;
        this.UpdateUIComponents();
    }

    public void TakeMoraleDamage(int moraleDamage)
    {
        var info = Unit.Info;
        this.Unit.Info.Morale.ChangeMorale(-moraleDamage);
        this.UpdateUIComponents();
    }

    public Coroutine AnimateDamaged(Color? color = null)
    {
        var renderer = this.GetComponent<SpriteRenderer>();
        return StartCoroutine(renderer.FlashColor(color ?? Color.red));
    }

    public Coroutine AnimateAttack()
    {
        return StartCoroutine(AnimateAttackInternal());
    }

    public Coroutine AnimateDeath()
    {
        return StartCoroutine(AnimateDeathInternal());
    }


    /// <summary>
    /// Shows text with the selected color and flashes in the chosen color
    /// </summary>
    public IEnumerator AnimateDamageTaken(string textNumber, Color textColor, Color flashColor)
    {
        this.CreateDamageTextOverHead(textNumber, textColor);
        yield return AnimateDamaged(flashColor);
    }

    public IEnumerator AnimateEscape(Vector3 direction)
    {
        var time = 0.0f;
        var speed = 3.0f;
        this.HideInterface();
        StartCoroutine(this.FadeAway());
        while (time < 1.5f)
        {
            var delta = TimeUtils.FullAdjustedGameDelta;
            time += delta;
            var pos = this.transform.position;
            this.transform.position = Vector3.MoveTowards(pos, pos + direction, speed * delta);
            yield return null;
        }
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


    private void CreateDamageTextOverHead(string text, Color? color = null)
    {
        var currentY = this.transform.position.y;
        var damageNumber = Instantiate(DamageTextTemplate);
        damageNumber.SetText(text, color);
        damageNumber.transform.position = this.transform.position.SetY(currentY + 0.25f);
    }

    /// <summary>
    /// Updates each part of the UI to the current player state
    /// </summary>
    private void UpdateUIComponents()
    {
        if (HealthBar == null)
        {
            HealthBar = GetComponentInChildren<HealthBar>();
        }

        var info = Unit.Info;

        // Update HP
        var hp = info.CurrentStats.HP;
        var percent = (float)hp / (float)info.MaxStats.HP;
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);
        HealthBar.SetHP(hp, percent);

        // Update Morale
        this.MoraleIcon.UpdateIcon(info.Morale);
    }

    public void HideInterface()
    {
        this.HealthBar.Hide();
        this.MoraleIcon.Hide();
    }
}
