﻿using FlavBattle.Combat;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CombatUnit : MonoBehaviour
{
    /// <summary>
    /// Floating icon type to show overhead
    /// </summary>
    public enum FloatingIconType
    {
        Shield,
        Morale,
    }

    public Unit Unit { get; private set; }

    [Required]
    public FloatingText DamageTextTemplate;

    [Required]
    public MoraleIcon MoraleIcon;

    [Required]
    public HealthBar HealthBar;

    [Tooltip("Animation for blocking an attack via high morale (flying morale icon)")]
    [Required]
    [SerializeField]
    private FloatingIcon _moraleTankAnimationTemplate;

    [Tooltip("Animation for blocking an attack via shields (flying shield icon)")]
    [Required]
    [SerializeField]
    private FloatingIcon _shieldTankAnimationTemplate;

    [SerializeField]
    private CombatBuffIcon[] _buffTemplates;

    [Tooltip("Component used to display buffs")]
    [SerializeField]
    private GameObject _buffPanel;

    [Tooltip("Icon which represents this guy being dead")]
    [SerializeField]
    [Required]
    private GameObject _skullIcon;

    private bool _facingLeft = false;

    private Animator _animator;
    private bool _animating = false;

    private AudioSource _audioSource;

    void Awake()
    {
        _animator = this.GetComponent<Animator>();
        _audioSource = this.GetComponent<AudioSource>();
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

        _animator.runtimeAnimatorController = unit.Data.Animator;

        this.UpdateUIComponents();
    }

    public void TakeDamage(int damage)
    {
        var info = Unit.Info;
        info.CurrentStats.HP -= damage;
    }

    public void TakeMoraleDamage(int moraleDamage)
    {
        var info = Unit.Info;
        this.Unit.Info.Morale.ChangeMorale(-moraleDamage);
    }

    public void AddBuff(CombatBuffIcon.BuffType type)
    {
        var template = _buffTemplates.FirstOrDefault(a => a.Type == type);
        if (template == null)
        {
            Debug.LogError($"No buff template of type {type}");
            return;
        }

        Instantiate(template, _buffPanel.transform);
    }

    public void RemoveBuff(CombatBuffIcon.BuffType type)
    {
        var buffs = _buffPanel.GetComponentsInChildren<CombatBuffIcon>();
        var buff = buffs.FirstOrDefault(a => a.Type == type);
        if (buff != null)
        {
            Destroy(buff.gameObject);
        }
    }

    public Coroutine AnimateFlash(Color? color = null)
    {
        var renderer = this.GetComponent<SpriteRenderer>();
        return StartCoroutine(renderer.FlashColor(color ?? Color.red));
    }

    public void AnimateDeath()
    {
        _animator.SetTrigger(UnitAnimatorTrigger.Die.ToString());
        this.MoraleIcon.Hide();
        this._skullIcon.Show();
    }

    public void AnimateShieldBlock()
    {
        this.PlayAnimator(UnitAnimatorTrigger.ShieldBlock);
    }

    public void AnimateFloatingIcon(FloatingIconType icon)
    {
        FloatingIcon template = null;
        switch (icon)
        {
            case FloatingIconType.Morale:
                template = _moraleTankAnimationTemplate;
                break;
            case FloatingIconType.Shield:
                template = _shieldTankAnimationTemplate;
                break;
            default:
                Debug.LogError("No implementation for icon " + icon);
                return;
        }

        if (template != null)
        {
            var anim = Instantiate(template, this.transform);
            anim.PlayAnimation();
        }
    }

    /// <summary>
    /// Shows text with the selected color and flashes in the chosen color
    /// </summary>
    public IEnumerator AnimateDamageTaken(string textNumber, Color textColor, Color flashColor)
    {
        this.CreateDamageTextOverHead(textNumber, textColor);
        yield return AnimateFlash(flashColor);
    }

    /// <summary>
    /// Plays the animator trigger without waiting for it to finish.
    /// </summary>
    /// <param name="animatorTrigger"></param>
    public void PlayAnimator(UnitAnimatorTrigger animatorTrigger)
    {
        this._animator.SetTrigger(animatorTrigger.ToString());
    }

    public IEnumerator PlayAnimatorToCompletion(UnitAnimatorTrigger animatorTrigger)
    {
        this._animating = true;
        this.PlayAnimator(animatorTrigger);
        yield return WaitForAnimationEnd();
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
    public void UpdateUIComponents()
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
        HealthBar.SetHPGradual(hp, percent);

        // Update Morale
        this.MoraleIcon.UpdateIcon(info.Morale);
    }

    public void HideInterface()
    {
        this.HealthBar.Hide();
        this.MoraleIcon.Hide();
    }

    // TRIGGERED BY ANIMATOR
    public void AnimationStarted()
    {
        this._animating = true;
    }

    // TRIGGERED BY ANIMATOR
    public void AnimationEnded()
    {
        this._animating = false;
    }

    /// <summary>
    /// Used to wait for animator animation to complete
    /// </summary>
    public IEnumerator WaitForAnimationEnd()
    {
        var timer = 0.0f;
        while (_animating)
        {
            timer += Time.deltaTime;
            if (timer > 5.0f)
            {
                Debug.LogError($" {this.Unit.Data.ClassName} attack took more than 5 seconds! Ensure animation has an AnimationEnded trigger!!");
                _animating = false;
                yield break;
            }

            yield return null;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
