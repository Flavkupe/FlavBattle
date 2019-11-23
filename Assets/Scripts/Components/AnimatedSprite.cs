using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
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
        if (idle)
        {
            _currentFrame = 0;
            _spriteRenderer.sprite = Animations[0];
        }
    }

    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    private SpriteRenderer _spriteRenderer;
    private int _currentFrame = 0;
    private bool _idle = true;
    private float _modifier = 1.0f;
    private float _timeout = 60.0f;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _spriteRenderer.sprite = Animations[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (_idle)
        {
            return;
        }

        _timeout += Time.deltaTime * AnimationSpeed * _modifier;
        if (_timeout >= 1.0f)
        {
            _timeout = 0;
            _currentFrame++;
            if (_currentFrame >= Animations.Length)
            {
                _currentFrame = 0;
            }

            _spriteRenderer.sprite = Animations[_currentFrame]; 
        }
    }
}
