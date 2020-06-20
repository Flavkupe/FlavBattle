using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FormationGroup
{
    All,
    Front,
    MidRow,
    Back,
    Left,
    Right,
    MidCol,
    BackAndMid,
    FrontAndMid,
    FourCorners,
    FourSides,
}

public static class FormationUtils
{
    public static FormationPair FL = new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Left };
    public static FormationPair FM = new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Middle };
    public static FormationPair FR = new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Right };
    public static FormationPair ML = new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Left };
    public static FormationPair MM = new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Middle };
    public static FormationPair MR = new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Right };
    public static FormationPair BL = new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Left };
    public static FormationPair BM = new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Middle };
    public static FormationPair BR = new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Right };

    public static FormationPair[] AllSquares =
    {
        FL, FM, FR,
        ML, MM, MR,
        BL, BM, BR,
    };

    public static FormationPair[] FrontRowSquares = { FL, FM, FR };
    public static FormationPair[] MiddleRowSquares = { ML, MM, MR };
    public static FormationPair[] BackRowSquares = { BL, BM, BR };
    public static FormationPair[] LeftColSquares = { FL, ML, BL };

    internal static List<FormationPair> GetIntersection(IEnumerable<FormationPair> a, IEnumerable<FormationPair> b)
    {
        return a.Intersect(b, EqualityComparer<FormationPair>.Default).ToList();
    }

    public static FormationPair[] MiddleColSquares = { FM, MM, BM };
    public static FormationPair[] RightColSquares = { FR, MR, BR };
    public static FormationPair[] FourCorners = { FR, FL, BR, BL };
    public static FormationPair[] FourSides = { MR, ML, FM, BM };

    public static FormationRow GetRow(FormationRowAndCol rowAndCol)
    {
        switch (rowAndCol)
        {
            case FormationRowAndCol.FL:
            case FormationRowAndCol.FM:
            case FormationRowAndCol.FR:
                return FormationRow.Front;
            case FormationRowAndCol.ML:
            case FormationRowAndCol.MM:
            case FormationRowAndCol.MR:
                return FormationRow.Middle;
            case FormationRowAndCol.BL:
            case FormationRowAndCol.BM:
            case FormationRowAndCol.BR:
            default:
                return FormationRow.Back;
        }
    }

    public static FormationColumn GetColumn(FormationRowAndCol rowAndCol)
    {
        switch (rowAndCol)
        {
            case FormationRowAndCol.FL:
            case FormationRowAndCol.ML:
            case FormationRowAndCol.BL:
                return FormationColumn.Left;
            case FormationRowAndCol.FM:
            case FormationRowAndCol.MM:
            case FormationRowAndCol.BM:
                return FormationColumn.Middle;
            case FormationRowAndCol.FR:
            case FormationRowAndCol.MR:
            case FormationRowAndCol.BR:
            default:
                return FormationColumn.Right;
        }
    }

    public static FormationPair GetPair(FormationRowAndCol rowAndCol)
    {
        return new FormationPair(GetRow(rowAndCol), GetColumn(rowAndCol));
    }

    public static FormationPair[] GetFormationPairs(FormationGroup group)
    {
        switch (group)
        {
            case FormationGroup.Front:
                return FrontRowSquares;
            case FormationGroup.MidRow:
                return MiddleRowSquares;
            case FormationGroup.Back:
                return BackRowSquares;
            case FormationGroup.Left:
                return LeftColSquares;
            case FormationGroup.Right:
                return RightColSquares;
            case FormationGroup.MidCol:
                return MiddleColSquares;
            case FormationGroup.BackAndMid:
                return BackRowSquares.Concat(MiddleRowSquares).ToArray();
            case FormationGroup.FrontAndMid:
                return FrontRowSquares.Concat(MiddleRowSquares).ToArray();
            case FormationGroup.FourCorners:
                return FourCorners;
            case FormationGroup.FourSides:
                return FourSides;
            case FormationGroup.All:
            default:
                return AllSquares;
        }
    }

    public static bool IncludesRow(FormationGroup group, FormationRow row)
    {
        return GetFormationPairs(group).Any(a => a.Row == row);
    }

    /// <summary>
    /// Orientation is clockwise, from top-most square (north of center).
    /// Orientation is relative to where unit is looking (that is,
    /// the Formation unit's left/right, etc), not where the opposing
    /// army is looking.
    /// </summary>
    public static readonly Dictionary<FormationOrientation, FormationPair[]> Orientations = new Dictionary<FormationOrientation, FormationPair[]> {
        {
            FormationOrientation.BottomRight,
            new FormationPair[] {
                BL, ML, FL, // 0 1 2
                BM, MM, FM, // 3 4 5
                BR, MR, FR  // 6 7 8
            }
        },
        {
            FormationOrientation.BottomLeft,
            new FormationPair[] {
                BR, BM, BL, // 0 1 2
                MR, MM, ML, // 3 4 5
                FR, FM, FL  // 6 7 8
            }
        }
    };

    private static IFormationGridSlot MakeSlot<TObjectType>(TObjectType grid, Vector3 localPos, FormationPair pair) where TObjectType : MonoBehaviour, IFormationGrid
    {
        var slot = grid.CreateSlot();
        slot.Row = pair.Row;
        slot.Col = pair.Col;
        slot.Instance.name = pair.Row.ToString() + pair.Col.ToString();
        slot.Instance.transform.SetParent(grid.transform);
        slot.Instance.transform.localPosition = localPos;
        return slot;
    }

    public static TObjectType PopulateFormationGrid<TObjectType>(TObjectType grid, FormationOrientation orientation = FormationOrientation.BottomRight, float gap = 2.0f) where TObjectType : MonoBehaviour, IFormationGrid
    {
        var xGap = gap * 0.75f;
        var yGap = gap * 0.5f;
        return PopulateFormationGrid(grid, orientation, xGap, yGap);
    }

    public static TObjectType PopulateFormationGrid<TObjectType>(TObjectType grid, FormationOrientation orientation, float xGap, float yGap) where TObjectType : MonoBehaviour, IFormationGrid
    {
        var orientations = Orientations[orientation];

        var tv = new Vector3(0.0f, yGap);
        var rv = new Vector3(xGap, 0.0f);
        var bv = new Vector3(0.0f, -yGap);
        var lv = new Vector3(-xGap, 0.0f);

        // Make sure to create them in order; this matters for stuff
        // like canvases. These are therefore top-to-bottom on hierarchy
        // based on whats in front of what
        MakeSlot(grid, tv, orientations[0]); // Top
        MakeSlot(grid, (lv + tv) / 2, orientations[3]); // TL
        MakeSlot(grid, (tv + rv) / 2, orientations[1]); // TR
        MakeSlot(grid, lv, orientations[6]); // Left
        MakeSlot(grid, Vector3.zero, MM); // MM
        MakeSlot(grid, rv, orientations[2]); // Right
        MakeSlot(grid, (lv + bv) / 2, orientations[7]); // BL
        MakeSlot(grid, (rv + bv) / 2, orientations[5]); // BR
        MakeSlot(grid, bv, orientations[8]); // Down

        return grid;
    }

    public static TObjectType CreateFormationGrid<TObjectType>(TObjectType template, FormationOrientation orientation = FormationOrientation.BottomRight, float gap = 2.0f) where TObjectType : MonoBehaviour, IFormationGrid
    {
        var grid = UnityEngine.Object.Instantiate(template);
        return PopulateFormationGrid(grid, orientation, gap);
    }
}

public interface IFormationGridSlot
{
    MonoBehaviour Instance { get; }

    FormationRow Row { get; set; }

    FormationColumn Col { get; set; }

    void SetUnit(Unit unit);
}

public interface IFormationGrid
{
    IFormationGridSlot CreateSlot();
}
