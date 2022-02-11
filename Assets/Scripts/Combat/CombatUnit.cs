using FlavBattle.Combat;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using FlavBattle.Entities;
using FlavBattle.Components;

/// <summary>
/// Visual and audio component of the Combatant. Does not modify
/// Unit's state, only shows graphical or audial representations
/// of the unit in combat.
/// </summary>
public class CombatUnit : MonoBehaviour, IPointerClickHandler
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
    [Tooltip("Original local position where AnimatedCharacter is instantiated.")]
    [SerializeField]
    private Transform _animatedCharPosition;

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
    private FlowLayout _buffPanel;

    [Tooltip("Icon which represents this guy being dead")]
    [SerializeField]
    [Required]
    private GameObject _skullIcon;

    [SerializeField]
    [Required]
    [Tooltip("Front position if the character is facing left")]
    private Transform _leftFront;

    [SerializeField]
    [Required]
    [Tooltip("Front position if the character is facing right")]
    private Transform _rightFront;

    [SerializeField]
    [Required]
    [Tooltip("Overlay with morale, health, etc")]
    private StatOverlay _statOverlay;

    [SerializeField]
    [Required]
    [Tooltip("Sword part of overlay (damage)")]
    private GameObject _overlaySwords;

    [SerializeField]
    [Required]
    [Tooltip("Shield part of overlay (defense)")]
    private GameObject _overlayShield;

    /// <summary>
    /// Throttle the stat update so it doesn't need to update too often.
    /// </summary>
    private ThrottleTimer _statUpdateThrottle = new ThrottleTimer(1.0f);


    /// <summary>
    /// Position representing the front of a character
    /// </summary>
    public Transform Front => this._facingLeft ? _leftFront : _rightFront;

    /// <summary>
    /// Position representing the back of a character
    /// </summary>
    public Transform Back => this._facingLeft ? _rightFront : _leftFront;

    /// <summary>
    /// Original position for the animated character.
    /// </summary>
    public Transform OriginalPos => _animatedCharPosition;

    private Animator _animator;

    public event EventHandler RightClicked;

    private AudioSource _audioSource;

    private AnimatedCharacter _animatedCharacter;
    /// <summary>
    /// The AnimatedCharacter instance for this unit, Instantiated in SetUnit.
    /// </summary>
    public AnimatedCharacter Character => _animatedCharacter;

    private bool _facingLeft;

    void Awake()
    {
        _audioSource = this.GetComponent<AudioSource>();
    }

    void Start()
    {
        _overlayShield.Hide();
        _overlaySwords.Hide();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ShowAndUpdateStatUI();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            
            _overlayShield.Hide();
            _overlaySwords.Hide();
        }
    }

    public void SetUnit(Unit unit, bool facingLeft)
    {
        this.Unit = unit;
        _facingLeft = facingLeft;

        var renderer = this.GetComponent<SpriteRenderer>();
        if (unit.Data.AnimatedCharacter != null)
        {
            // use custom AnimatedCharacter instead of animator, if included
            renderer.sprite = null;
            renderer.enabled = false;

            var currentAnimator = this.GetComponent<Animator>();
            currentAnimator.enabled = false;

            _animatedCharacter = Instantiate(unit.Data.AnimatedCharacter, this.transform, false);
            _animatedCharacter.transform.localPosition = _animatedCharPosition.localPosition;
            _animator = _animatedCharacter.Animator;
            _animatedCharacter.SetFlippedLeft(facingLeft);
        }

        this.UpdateUIComponents();
    }

    public void AddBuffIcon(CombatBuffIcon.BuffType type, int duration = 0)
    {
        var template = _buffTemplates.FirstOrDefault(a => a.Type == type);
        if (template == null)
        {
            Debug.LogError($"No buff template of type {type}");
            return;
        }

        var icon = Instantiate(template);
        _buffPanel.AddObject(icon.gameObject);
    }

    public void RemoveBuffIcon(CombatBuffIcon.BuffType type)
    {
        var buffs = _buffPanel.GetComponentsInChildren<CombatBuffIcon>();
        var buff = buffs.FirstOrDefault(a => a.Type == type);
        if (buff != null)
        {
            _buffPanel.RemoveObject(buff.gameObject, true);
        }
    }

    public Coroutine AnimateFlash(Color? color = null)
    {
        return StartCoroutine(this._animatedCharacter.FlashColor(color ?? Color.red));
    }

    public void AnimateDeath()
    {
        PlayAnimator(UnitAnimatorTrigger.Die);
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
    /// Shows text overhead with the selected color
    /// </summary>
    public void AnimateOverheadText(string text, Color textColor)
    {
        this.CreateDamageTextOverHead(text, textColor);
    }

    /// <summary>
    /// Plays the animator trigger without waiting for it to finish.
    /// </summary>
    /// <param name="animatorTrigger"></param>
    public void PlayAnimator(UnitAnimatorTrigger animatorTrigger, float speed = 1.0f)
    {
        if (_animatedCharacter != null)
        {
            _animatedCharacter.PlayAnimation(animatorTrigger, speed);
        }
    }

    public IEnumerator PlayAnimatorToCompletion(UnitAnimatorTrigger animatorTrigger, float speed = 1.0f)
    {
        // TODO: other states too
        var state = UnitAnimatorState.AttackState;

        // if already in state, wait for it to finish animation
        if (IsInAnimationState(state))
        {
            yield return WaitForAnimationEnd(state);
        }

        var startingSpeed = this._animatedCharacter.Speed;

        this.PlayAnimator(animatorTrigger, speed);

        // wait 2 frames for the animation state to change
        yield return Utils.WaitUntilNextFrame(2);

        // now actually play animation to completion
        yield return WaitForAnimationEnd(state);

        this.SetSpeed(startingSpeed);
    }

    public void SetSpeed(float speed)
    {
        this._animatedCharacter.SetSpeedModifier(speed);
    }

    public IEnumerator AnimateEscape(Vector3 direction)
    {
        var time = 0.0f;
        var speed = 3.0f;
        this.HideInterface();
        StartCoroutine(this.FadeAway());

        // flip direction
        this._animatedCharacter.SetFlippedLeft(!this._facingLeft);
        this._animatedCharacter.PlayAnimation(UnitAnimatorTrigger.Run);
        while (time < 1.5f)
        {
            var delta = TimeUtils.FullAdjustedGameDelta;
            time += delta;
            var pos = this._animatedCharacter.transform.position;
            this._animatedCharacter.transform.position = Vector3.MoveTowards(pos, pos + direction, speed * delta);
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
        _statOverlay.Hide();
    }

    /// <summary>
    /// Used to wait for animator animation to complete
    /// </summary>
    public IEnumerator WaitForAnimationEnd(UnitAnimatorState state = UnitAnimatorState.AttackState)
    {
        var timer = 0.0f;
        while (IsInAnimationState(state))
        {
            timer += Time.deltaTime;
            if (timer > 5.0f)
            {
                Debug.LogError($" {this.Unit.Data.ClassName} attack took more than 5 seconds! Ensure animation has an AnimationEnded trigger!!");
                yield break;
            }

            yield return null;
        }
    }

    public bool IsInAnimationState(UnitAnimatorState state)
    {
        return _animatedCharacter.IsInState(state);
    }

    public void PlaySound(AudioClip clip)
    {
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClicked?.Invoke(this, new EventArgs());
        }
    }

    private void ShowAndUpdateStatUI()
    {
        if (!_overlayShield.IsShowing())
        {
            _overlayShield.Show();
        }

        if (!_overlaySwords.IsShowing())
        {
            _overlaySwords.Show();
        }

        if (Unit != null && _statUpdateThrottle.Tick())
        {
            var summary = Unit.GetStatSummary();
            _statOverlay.UpdateOverlay(summary);
        }
    }
}
