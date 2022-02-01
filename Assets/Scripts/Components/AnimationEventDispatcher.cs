using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using FlavBattle.Combat;

[Serializable]
public class UnityAnimationEvent : UnityEvent<string> { };

public class AnimationEventDispatcher : MonoBehaviour
{
    [SerializeField]
    [Required]
    private Animator _animator;

    public UnityAnimationEvent OnAnimationStart;
    public UnityAnimationEvent OnAnimationComplete;
    public UnityEvent<UnitAnimatorEvent> OnAnimationEvent;

    public void AnimationStartHandler(string name)
    {
        OnAnimationStart?.Invoke(name);
    }
    public void AnimationCompleteHandler(string name)
    {
        OnAnimationComplete?.Invoke(name);
    }


    public void AnimationEvent(UnitAnimatorEvent animationEvent)
    {
        OnAnimationEvent?.Invoke(animationEvent);
    }
}
