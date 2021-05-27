using FlavBattle.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitGenerator
{
    public class RandomArmyOptions
    {
        public int MinLevel = 1;
        public int MaxLevel = 1;
        public int MinUnitNum = 1;
        public int MaxUnitNum = 1;
    }

    public static Unit MakeUnit(Faction faction, int level = 1, bool isOfficer = false)
    {
        return MakeUnit(ResourceHelper.Units.GetRandom(), faction, level, isOfficer);
    }

    public static Unit MakeUnit(UnitData data, Faction faction, int level = 1, bool isOfficer = false)
    {
        if (data == null)
        {
            return null;
        }

        var unit = new Unit()
        {
            Data = data,
            Info = new UnitInfo(data, faction, level, isOfficer)
        };

        return unit;
    }

    /// <summary>
    /// Populates an army with random units and levels in random open positions.
    /// Always creates a random officer first.
    /// </summary>
    public static void PopulateArmy(IArmy army, Faction faction, RandomArmyOptions options)
    {
        var rand = Random.Range(options.MinUnitNum, options.MaxUnitNum + 1);
        var officer = MakeUnit(faction, options.MaxLevel, true);
        army.Formation.PutUnit(officer);
        for (var i = 1; i < rand; i++)
        {
            var randLevel = Random.Range(options.MinLevel, options.MaxLevel + 1);
            var unit = MakeUnit(faction, randLevel);
            army.Formation.PutUnit(unit);
        }
    }
}
