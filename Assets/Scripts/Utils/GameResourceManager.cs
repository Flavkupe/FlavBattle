﻿using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameResourceManager : SingletonObject<GameResourceManager>
{
    private void Awake()
    {
        this.SetSingleton(this);
    }

    [Required]
    [Tooltip("The default idle ability if no ability can be done")]
    [SerializeField]
    private CombatAbilityData DefaultIdleAbility;

    /// <summary>
    /// Gets the default action if nothing is possible.
    /// </summary>
    /// <returns></returns>
    public CombatAction GetDefaultCombatAction()
    {
        return new CombatAction()
        {
            Ability = DefaultIdleAbility,
            Priority = CombatAbilityPriority.LastResort,
            Target = new CombatTargetInfo()
            {
                TargetType = CombatAbilityTarget.Self,
            }
        };
    }

    [Serializable]
    public class CommonSoundPrefabs
    {
        [Required]
        public AudioClip SelectSound;

        [Required]
        public AudioClip FanfareSound;
    }

    [Serializable]
    public class UISoundPrefabs
    {
        [Required]
        public AudioClip Close;

        [Required]
        public AudioClip Open;
    }

    [BoxGroup("Sounds")]
    public CommonSoundPrefabs CommonSounds;

    [BoxGroup("Sounds")]
    public UISoundPrefabs UISounds;
}

/// <summary>
/// Shortcut to GameResourceManager.Instance for common prefab properties
/// </summary>
public static class GRM
{
    public static GameResourceManager.CommonSoundPrefabs CommonSounds => GameResourceManager.Instance.CommonSounds;

    public static GameResourceManager.UISoundPrefabs UISounds => GameResourceManager.Instance.UISounds;
}