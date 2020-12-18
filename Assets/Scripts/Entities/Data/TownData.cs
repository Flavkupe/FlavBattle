using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Town Data", menuName = "Custom/Props/Town Data", order = 1)]
public class TownData : ScriptableObject
{
    public FactionData StartingFaction;

    [Required]
    public FloatingText RedTextTemplate;

    [Required]
    public AudioClip GoodConqueredClip;

    [Required]
    public AudioClip BadConqueredClip;

    [AssetIcon]
    public Sprite Icon;

    public float RequiredToTake = 20.0f;
}
