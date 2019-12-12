using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static FormationPair[] MiddleColSquares = { FM, MM, BM };
    public static FormationPair[] RightColSquares = { FR, MR, BR };
    public static FormationPair[] FourCorners = { FR, FL, BR, BL };
    public static FormationPair[] FourSides = { MR, ML, FM, BM };

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
        var grid = Object.Instantiate(template);
        return PopulateFormationGrid(grid, orientation, gap);
    }
}

public interface IFormationGridSlot
{
    MonoBehaviour Instance { get; }

    FormationRow Row { get; set; }

    FormationColumn Col { get; set; }
}

public interface IFormationGrid
{
    IFormationGridSlot CreateSlot();
}
