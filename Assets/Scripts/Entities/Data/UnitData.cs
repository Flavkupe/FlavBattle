using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Custom/Units/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 HP;

    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Power;

    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Defense;

    [MinMaxSlider(1.0f, 200.0f)]
    public Vector2 Speed;

    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 HPScaling;

    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 PowerScaling;

    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 DefenseScaling;

    [MinMaxSlider(1.0f, 20.0f)]
    public Vector2 SpeedScaling;

    [AssetIcon]
    public Sprite Icon;

    public Sprite Sprite;

    public Sprite[] Portraits;

    public Sprite[] Animations;

    public string Name;

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
