using NaughtyAttributes;
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

    [AssetIcon]
    public Sprite Icon;

    public Sprite Sprite;

    public string Name;

    public override string ToString()
    {
        return $"Unit_{Name}";
    }

    /// <summary>
    /// Roll unit stats from the possible base props
    /// </summary>
    /// <returns></returns>
    public UnitStats RollStats()
    {
        var stats = new UnitStats();
        stats.HP = GenerateStat(HP);
        stats.Power = GenerateStat(Power);
        stats.Defense = GenerateStat(Defense);
        return stats;
    }

    private int GenerateStat(Vector2 stat)
    {
        return (int)Mathf.Round(Utils.MathUtils.RandomNormalBetween(stat.x, stat.y));
    }
}
