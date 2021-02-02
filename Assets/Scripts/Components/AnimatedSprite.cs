using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IAnimatedSprite
{
    void SetFlipped(bool flipped);
    void SetIdle(bool idle);
    void SetAnimations(Sprite[] animations);
    void SetColor(Color color);
    void SetSpeedModifier(float modifier);
}

public class AnimatedSprite : MonoBehaviour, IAnimatedSprite
{
    public Sprite[] Animations;

    /// <summary>
    /// How quickly the animaiton happens. 10
    /// is instantaneous.
    /// </summary>
    [Range(1.0f, 60.0f)]
    public float AnimationSpeed = 5.0f;

    /// <summary>
    /// Sets a modifier, such as 2.0f or 0.5f, which affects
    /// how much faster or slower the unit animates.
    /// </summary>
    public void SetSpeedModifier(float modifier)
    {
        _modifier = modifier;
    }

    public void SetIdle(bool idle)
    {
        _idle = idle;
        if (idle && Animations.Length > 0)
        {
            _currentFrame = 0;
            SetSprite(Animations[0]);
        }
    }

    public void SetFlipped(bool flipped)
    {
        if (this._spriteRenderer != null)
        {
            this._spriteRenderer.flipX = flipped;
        }
    }

    public void SetAnimations(Sprite[] animations)
    {
        Animations = animations;
    }

    public void SetColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
        else if (_image)
        {
            _image.color = color;
        }
    }

    private SpriteRenderer _spriteRenderer;
    private Image _image;
    private int _currentFrame = 0;
    private bool _idle = true;
    private float _modifier = 1.0f;
    private float _timeout = 60.0f;

    private void SetSprite(Sprite sprite)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = sprite;
        }
        else if (_image)
        {
            _image.sprite = sprite;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = this.GetComponent<SpriteRenderer>() ?? this.GetComponentInChildren<SpriteRenderer>();
        _image = this.GetComponent<Image>() ?? this.GetComponentInChildren<Image>();
        this.SetSprite(Animations[0]);
    }

    void Update()
    {
        if (_idle)
        {
            return;
        }

        _timeout += TimeUtils.FullAdjustedGameDelta * AnimationSpeed * _modifier;
        if (_timeout >= 1.0f)
        {
            _timeout = 0;
            _currentFrame++;
            if (_currentFrame >= Animations.Length)
            {
                _currentFrame = 0;
            }

            this.SetSprite(Animations[_currentFrame]);
        }
    }
}
