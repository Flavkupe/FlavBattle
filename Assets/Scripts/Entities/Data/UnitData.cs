using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [ShowAssetPreview(128, 128)]
    [AssetIcon]
    public Sprite Icon;

    public string Name;

    public string ClassName;

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

    public override string ToString()
    {
        return $"Unit_{Name}";
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

    public Sprite RollPortrait()
    {
        if (this.Portraits.Length == 0)
        {
            Debug.LogWarning($"No portraits for unit {this.ToString()}!");
            return null;
        }

        return Portraits.GetRandom();
    }

    public string RollName()
    {
        return this.Name;
    }

    private int GenerateStat(Vector2 stat)
    {
        return (int)Mathf.Round(Utils.MathUtils.RandomNormalBetween(stat.x, stat.y));
    }
}
