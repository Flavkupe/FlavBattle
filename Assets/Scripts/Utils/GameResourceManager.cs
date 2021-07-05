using NaughtyAttributes;
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

    [BoxGroup("Game Constants")]
    [Tooltip("Base threshold for an army to flee. Army will flee if morale is less than this at a certain turn.")]
    [SerializeField]
    private int _fleeingArmyThreshold = 75;
    public int FleeingArmyThreshold => _fleeingArmyThreshold;


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


    [Serializable]
    public class CombatSoundPrefabs
    {
        [Required]
        public AudioClip[] Block;
    }


    [Serializable]
    public class CommonSpritePrefabs
    {
        [Tooltip("For exmaple for stance")]
        [Required]
        [AllowNesting]
        public Sprite OffenseIcon;

        [Tooltip("For exmaple for stance")]
        [Required]
        [AllowNesting]
        public Sprite DefenseIcon;

        [Tooltip("For exmaple for stance")]
        [Required]
        [AllowNesting]
        public Sprite NeutralIcon;

        [Tooltip("For things identified by time")]
        [Required]
        [AllowNesting]
        public Sprite SandClockIcon;
    }

    [BoxGroup("Sounds")]
    public CommonSoundPrefabs CommonSounds;

    [BoxGroup("Sounds")]
    public UISoundPrefabs UISounds;

    [BoxGroup("Sounds")]
    public CombatSoundPrefabs CombatSounds;

    [BoxGroup("Sprites")]
    public CommonSpritePrefabs CommonSprites;

}

/// <summary>
/// Shortcut to GameResourceManager.Instance for common prefab properties
/// </summary>
public static class GRM
{
    public static GameResourceManager Instance => GameResourceManager.Instance;

    public static GameResourceManager.CommonSpritePrefabs CommonSprites => GameResourceManager.Instance.CommonSprites;

    public static GameResourceManager.CommonSoundPrefabs CommonSounds => GameResourceManager.Instance.CommonSounds;

    public static GameResourceManager.UISoundPrefabs UISounds => GameResourceManager.Instance.UISounds;

    public static GameResourceManager.CombatSoundPrefabs CombatSounds => GameResourceManager.Instance.CombatSounds;
}
