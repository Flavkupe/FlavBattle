using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Shows a full cycle of the particle engine
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAnimation : PlayableAnimation
{
    [Tooltip("Starting offset when this is created")]
    public Vector3 Offset;

    private float _duration = 0.0f;

    private void Awake()
    {
        var particles = this.gameObject.GetComponent<ParticleSystem>();
        particles.Stop();
    }

    public override void PlayAnimation()
    {
        StartCoroutine(PlayToCompletion());
    }

    public override IEnumerator PlayToCompletion()
    {
        var particles = this.gameObject.GetComponent<ParticleSystem>();
        this.transform.position += Offset;
        this._duration = particles.main.duration;
        particles.Play();
        while (this._duration > 0.0f)
        {
            this._duration -= Time.deltaTime;
            yield return null;
        }

        particles.gameObject.Hide();
        Destroy(this.gameObject, 1.0f);
    }
}
