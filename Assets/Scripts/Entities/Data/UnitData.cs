using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [ShowAssetPreview(128, 128)]
    [AssetIcon]
    public Sprite Icon;

    public string ClassName;

    [Tooltip("Debug option to always use lowest roll for stat rolls.")]
    public bool RollLow = false;

    // CombatStrategyData
    [BoxGroup("Base Stats")]
    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 HP;

    [BoxGroup("Base Stats")]
    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Power;

    [BoxGroup("Base Stats")]
    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Defense;

    [BoxGroup("Base Stats")]
    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Speed;

    [BoxGroup("Stat Scaling")]
    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 HPScaling;

    [BoxGroup("Stat Scaling")]
    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 PowerScaling;

    [BoxGroup("Stat Scaling")]
    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 DefenseScaling;

    [BoxGroup("Stat Scaling")]
    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 SpeedScaling;

    [BoxGroup("Visual")]
    public Sprite Sprite;

    [BoxGroup("Visual")]
    public Sprite[] Portraits;

    [BoxGroup("Visual")]
    public Sprite[] Animations;

    [BoxGroup("Abilities")]
    public CombatStrategyData DefaultStrategy;

    [BoxGroup("Abilities")]
    public CombatAbilityData[] StartingAbilities;

    [BoxGroup("Abilities")]
    [Tooltip("Officer abilities that are always available by default")]
    public OfficerAbilityData[] DefaultOfficerAbilities;

    [BoxGroup("Abilities")]
    [Tooltip("Officer abilities that can be learned")]
    public OfficerAbilityData[] OfficerAbilities;

    private string _name = "Unnamed";

    public override string ToString()
    {
        return $"Unit_{_name}";
    }

    /// <summary>
    /// Roll unit stats from the possible base props
    /// </summary>
    /// <returns></returns>
    public UnitStats RollStats(int level)
    {
        var stats = new UnitStats();
        stats.Level = level;
        stats.HP = GenerateStat(HP);
        stats.Power = GenerateStat(Power);
        stats.Defense = GenerateStat(Defense);
        stats.Speed = GenerateStat(Speed);
        for (int i = 1; i < level; i++)
        {
            var levelup = RollLevel();
            stats = stats.Combine(levelup);
        }

        return stats;
    }

    public UnitStats RollLevel()
    {
        var stats = new UnitStats();
        stats.HP = GenerateStat(HPScaling);
        stats.Power = GenerateStat(PowerScaling);
        stats.Defense = GenerateStat(DefenseScaling);
        stats.Speed = GenerateStat(SpeedScaling);
        return stats;
    }

    public OfficerAbilityData RollNewOfficerAbility(int level, List<OfficerAbilityData> existing)
    {
        if (OfficerAbilities == null || OfficerAbilities.Length == 0)
        {
            return null;
        }

        // Find abilities that match min level and are not already known
        var available = OfficerAbilities.Where(a => a.MinLevel <= level && !existing.Any(b => b.Name == a.Name)).ToList();
        if (available.Count == 0)
        {
            // no viable abilities
            return null;
        }

        return available.GetRandom();
    }

    public Sprite RollPortrait()
    {
        if (this.Portraits.Length == 0)
        {
            Debug.LogWarning($"No portraits for unit {this.ToString()}!");
            return null;
        }

        return Portraits.GetRandom();
    }

    /// <summary>
    /// Sets a name
    /// </summary>
    public string RollName()
    {
        // TODO: Data-driven names
        var _name = new List<string>
        {
            "Flavio",
            "Bob",
            "Blerb",
            "Bork",
            "Blahb"
        }.GetRandom();

        return _name;
    }

    private int GenerateStat(Vector2 stat)
    {
        if (RollLow)
        {
            return (int)stat.x;
        }

        return (int)Mathf.Round(Utils.MathUtils.RandomNormalBetween(stat.x, stat.y));
    }
}
