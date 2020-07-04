using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public interface IPlayableAnimation
{
    IEnumerator PlayToCompletion();

    void PlayAnimation();

    float Speed { get; set; }

    GameObject Instance { get; }
}

public abstract class PlayableAnimation : MonoBehaviour, IPlayableAnimation
{
    public float Speed { get { return _speed; } set { _speed = value; } }

    [Tooltip("How quickly this moves up")]
    [SerializeField]
    private float _speed = 1.0f;

    public GameObject Instance => this.gameObject;

    public abstract void PlayAnimation();

    public abstract IEnumerator PlayToCompletion();
}