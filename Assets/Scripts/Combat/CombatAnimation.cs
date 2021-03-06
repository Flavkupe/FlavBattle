﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animation))]
public class CombatAnimation : PlayableAnimation
{
    private Animation _animation;

    public override bool ScaleToTarget => true;

    protected virtual void Awake()
    {
        _animation = GetComponent<Animation>();
    }

    public override void PlayAnimation()
    {
        _animation.Play();
    }

    public override IEnumerator PlayToCompletion()
    {
        foreach (AnimationState state in _animation)
        {
            state.speed = Speed;
        }

        _animation.Play();

        while (_animation.isPlaying)
        {
            yield return null;
        }
    }
}
