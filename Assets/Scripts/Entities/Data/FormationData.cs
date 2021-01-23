using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InitialFormation
{
    public int Level = 1;

    public UnitData FL;
    public UnitData FM;
    public UnitData FR;

    public UnitData ML;
    public UnitData MM;
    public UnitData MR;

    public UnitData BL;
    public UnitData BM;
    public UnitData BR;

    public FormationRowAndCol OfficerPosition;

    public UnitData GetOfficer()
    {
        return GetUnit(OfficerPosition);
    }

    public UnitData GetUnit(FormationRowAndCol rowAndCol)
    {
        switch (rowAndCol)
        {
            case FormationRowAndCol.FL:
                return FL;
            case FormationRowAndCol.FM:
                return FM;
            case FormationRowAndCol.FR:
                return FR;
            case FormationRowAndCol.ML:
                return ML;
            case FormationRowAndCol.MM:
                return MM;
            case FormationRowAndCol.MR:
                return MR;
            case FormationRowAndCol.BL:
                return BL;
            case FormationRowAndCol.BM:
                return BM;
            case FormationRowAndCol.BR:
            default:
                return BR;
        }
    }

    public Formation CreateFormation(Faction faction)
    {
        var officer = GetOfficer();
        if (officer == null)
        {
            Debug.LogError("Officer is null for formation data!");
            return null;
        }

        var formation = new Formation();
        foreach (var pos in (FormationRowAndCol[])Enum.GetValues(typeof(FormationRowAndCol)))
        {
            var unitData = GetUnit(pos);
            if (unitData != null)
            {
                var unit = UnitGenerator.MakeUnit(unitData, faction, Level, unitData == officer);
                formation.PutUnit(unit, FormationUtils.GetPair(pos));
            }
        }

        return formation;
    }
}

[CreateAssetMenu(fileName = "Formation Data", menuName = "Custom/Units/Formation Data", order = 1)]
public class FormationData : ScriptableObject
{
    public InitialFormation InitialFormation;

    [AssetIcon]
    public Sprite Icon => InitialFormation?.GetOfficer()?.Sprite;

    public Formation CreateFormation(Faction faction)
    {
        return InitialFormation?.CreateFormation(faction);
    }
}
