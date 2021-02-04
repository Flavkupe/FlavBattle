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
    void ToggleSpriteVisible(bool visible);
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
        if (this.SpriteRenderer != null)
        {
            this.SpriteRenderer.flipX = flipped;
        }
    }

    public void SetAnimations(Sprite[] animations)
    {
        Animations = animations;
    }

    public void SetColor(Color color)
    {
        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = color;
        }
        else if (_image)
        {
            _image.color = color;
        }
    }

    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer SpriteRenderer => this.GetCachedComponent(ref _spriteRenderer);
    
    private Image _image;
    private int _currentFrame = 0;
    private bool _idle = true;
    private float _modifier = 1.0f;
    private float _timeout = 60.0f;

    private void SetSprite(Sprite sprite)
    {
        if (SpriteRenderer != null)
        {
            SpriteRenderer.sprite = sprite;
        }
        else if (_image)
        {
            _image.sprite = sprite;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _image = this.GetComponent<Image>() ?? this.GetComponentInChildren<Image>();
        if (Animations.Length > 0)
        {
            this.SetSprite(Animations[0]);
        }
    }

    void Update()
    {
        if (_idle)
        {
            return;
        }

        if (Animations.Length == 0)
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

    public void ToggleSpriteVisible(bool visible)
    {
        if (SpriteRenderer != null)
        {
            SpriteRenderer.enabled = visible;
        }
        else if (_image != null)
        {
            _image.enabled = visible;
        }
    }
}
