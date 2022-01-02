using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/// <summary>
/// Credit to: https://gamedev.stackexchange.com/questions/117423/unity-detect-animations-end
/// </summary>
[Serializable]
public class UnityAnimationEvent : UnityEvent<string> { };

public class AnimationEventDispatcher : MonoBehaviour
{
    [SerializeField]
    [Required]
    private Animator _animator;

    public UnityAnimationEvent OnAnimationStart;
    public UnityAnimationEvent OnAnimationComplete;


    [SerializeField]
    [Tooltip("Action names that are fully ignored")]
    private string[] _ignoreList;

    private HashSet<string> _ignoreSet = new HashSet<string>();

    void Awake()
    {
        _ignoreSet = new HashSet<string>(_ignoreList);

        for (int i = 0; i < _animator.runtimeAnimatorController.animationClips.Length; i++)
        {                
            AnimationClip clip = _animator.runtimeAnimatorController.animationClips[i];

            var ignore = false;
            foreach (var clipEvent in clip.events)
            {
                // Already set earlier
                if (clipEvent.functionName == "AnimationStartHandler")
                {
                    ignore = true;
                    break;
                }
            }

            if (ignore)
            {
                continue;
            }

            Debug.Log("clip: " + clip.name);

            AnimationEvent animationStartEvent = new AnimationEvent();
            animationStartEvent.time = 0;
            animationStartEvent.functionName = "AnimationStartHandler";
            animationStartEvent.stringParameter = clip.name;

            AnimationEvent animationEndEvent = new AnimationEvent();
            animationEndEvent.time = clip.length;
            animationEndEvent.functionName = "AnimationCompleteHandler";
            animationEndEvent.stringParameter = clip.name;

            clip.AddEvent(animationStartEvent);
            clip.AddEvent(animationEndEvent);
        }
    }

    public void AnimationStartHandler(string name)
    {
        if (_ignoreSet.Contains(name))
        {
            return;
        }

        Debug.Log($"{name} animation start.");
        OnAnimationStart?.Invoke(name);
    }
    public void AnimationCompleteHandler(string name)
    {
        if (_ignoreSet.Contains(name))
        {
            return;
        }

        Debug.Log($"{name} animation complete.");
        OnAnimationComplete?.Invoke(name);
    }
}
