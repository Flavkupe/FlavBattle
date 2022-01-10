using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoraleIcon : MonoBehaviour
{
    public ParticleSystem GainParticles;

    public ParticleSystem LossParticles;

    [SerializeField]
    private bool _tooltipEnabled = true;

    void Start()
    {
        if (_tooltipEnabled && !this.HasComponent<TooltipSource>())
        {
            Debug.LogWarning("Component does not have a tooltip source!");
        }
    }

    public void UpdateIcon(Morale morale)
    {
        var color = morale.GetColor();
        this.SetColor(color);

        if (_tooltipEnabled)
        {
            var tooltip = GetComponent<TooltipSource>();
            tooltip.TooltipText = morale.Current.ToString();
        }
    }

    public void AnimatePositiveChange()
    {
        if (GainParticles != null)
        {
            GainParticles.Play();
        }
    }

    public void AnimateNegativeChange()
    {
        if (LossParticles != null)
        {
            LossParticles.Play();
        }
    }
}
