using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public enum BarType
    {
        Bar,
        Radial,
    }

    public BarType Type;

    private bool IsBarType() => Type == BarType.Bar;

    [Tooltip("The colored graphic of the health bar")]
    public SpriteRenderer Bar;

    [HideIf("IsBarType")]
    [Tooltip("The sprite mask, for the Radial mask type")]
    public SpriteMask Mask;

    [ShowIf("IsBarType")]
    [Tooltip("The actual visual component that will shrink as health changes, for Bar type (does not affect colliders etc)")]
    public GameObject Visual;

    /// <summary>
    /// Percent currently displayed on the UI
    /// </summary>
    private float CurrentPercent
    {
        get
        {
            return Type == BarType.Bar ? Visual.transform.localScale.x : 1.0f - Mask.alphaCutoff;
        }
    }

    // Target values for changing the percents gradually rather than instantly.
    private float? _targetPercent = null;
    private int _targetHP = 0;
    private float _gradualSpeed = 3.0f;

    void Update()
    {
        if (_targetPercent.HasValue)
        {
            if (Mathf.Abs(_targetPercent.Value - CurrentPercent) > 0.05f)
            {
                var newPercent = Mathf.Lerp(CurrentPercent, _targetPercent.Value, Time.deltaTime * _gradualSpeed);
                SetHP(_targetHP, newPercent);
            }
            else
            {
                SetHP(_targetHP, _targetPercent.Value);
                _targetPercent = null;
            }
        }
    }

    public void SetHPGradual(int hp, float percent, float speed = 3.0f)
    {
        _targetPercent = percent;
        _gradualSpeed = speed;
        _targetHP = hp;
    }

    public void SetHP(int hp, float percent)
    {
        Debug.Assert(percent >= 0.0f && percent <= 1.0f);

        if (percent > 0.75f)
        {
            this.Bar.color = Color.green;
        }
        else if (percent >= 0.5f)
        {
            this.Bar.color = Color.yellow;
        }
        else
        {
            this.Bar.color = Color.red;
        }

        if (Type == BarType.Bar)
        {
            Visual.transform.localScale = Visual.transform.localScale.SetX(percent);
        }
        else if (Type == BarType.Radial)
        {
            Mask.alphaCutoff = Mathf.Max(0.01f, 1.0f - percent);
        }

        var tooltip = GetComponent<TooltipSource>();
        if (tooltip != null)
        {
            tooltip.TooltipText = hp.ToString();
        }
    }
}
