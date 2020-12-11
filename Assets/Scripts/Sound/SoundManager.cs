using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommonSoundType
{
    Select,
}

public enum UISoundType
{
    Open,
    Close,
}

public class SoundManager : SingletonObject<SoundManager>
{
    [Serializable]
    public class CommonSounds
    {
        [Required]
        public AudioClip SelectSound;
    }

    [Serializable]
    public class UISounds
    {
        [Required]
        public AudioClip Close;

        [Required]
        public AudioClip Open;
    }

    [Required]
    public AudioSource GlobalSource;

    public CommonSounds Common;
    public UISounds UI;

    void Awake()
    {
        this.SetSingleton(this);
    }

    public void PlaySound(CommonSoundType type)
    {
        var clip = GetCommonSoundClip(type);
        if (clip != null)
        {
            PlayClip(clip);
        }
    }

    public void PlaySound(UISoundType type)
    {
        var clip = GetUISoundClip(type);
        if (clip != null)
        {
            PlayClip(clip);
        }
    }

    public void PlayClip(AudioClip clip)
    {
        if (GlobalSource == null)
        {
            Debug.LogError("No GlobalSource instance!");
            return;
        }

        GlobalSource.PlayOneShot(clip);
    }

    private AudioClip GetCommonSoundClip(CommonSoundType type)
    {
        switch (type)
        {
            case CommonSoundType.Select:
                return Common.SelectSound;
            default:
                Debug.LogError("No sound effect set for " + type);
                return null;
        }
    }

    private AudioClip GetUISoundClip(UISoundType type)
    {
        switch (type)
        {
            case UISoundType.Close:
                return UI.Close;
            case UISoundType.Open:
                return UI.Open;
            default:
                Debug.LogError("No sound effect set for " + type);
                return null;
        }
    }
}

public static class Sounds
{
    public static void Play(AudioClip clip)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("No SoundManager instance!");
            return;
        }

        SoundManager.Instance.PlayClip(clip);
    }

    public static void Play(CommonSoundType type)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("No SoundManager instance!");
            return;
        }

        SoundManager.Instance.PlaySound(type);
    }

    public static void Play(UISoundType type)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("No SoundManager instance!");
            return;
        }

        SoundManager.Instance.PlaySound(type);
    }
}