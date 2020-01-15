using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animation))]
public class CombatAnimation : MonoBehaviour
{
    private Animation _animation;

    public float Speed = 1.0f;

    private void Awake()
    {
        _animation = GetComponent<Animation>();
    }

    public void PlayAnimation()
    {
        _animation.Play();
    }

    public IEnumerator PlayToCompletion()
    {
        foreach (AnimationState state in _animation)
        {
            state.speed = Speed;
        }

        _animation.Play();

        while(_animation.isPlaying)
        {
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
