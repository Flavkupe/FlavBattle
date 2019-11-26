using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationRow
{
    Front = 0,
    Middle = 1,
    Back = 2,
}

public enum FormationColumn
{
    Left = 0,
    Middle = 1,
    Right = 2,
}

public struct FormationPair
{
    public FormationRow Row;
    public FormationColumn Col;
}

public class Formation
{
    public static FormationPair[] AllSquares =
    {
        new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Left },
        new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Middle },
        new FormationPair { Row = FormationRow.Front, Col = FormationColumn.Right },
        new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Left },
        new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Middle },
        new FormationPair { Row = FormationRow.Middle, Col = FormationColumn.Right },
        new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Left },
        new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Middle },
        new FormationPair { Row = FormationRow.Back, Col = FormationColumn.Right },
    };

    private Unit[,] _units = new Unit[3,3];

    public IEnumerable<Unit> GetUnits()
    {
        var list = new List<Unit>();
        foreach (var unit in this._units)
        {
            if (unit != null)
            {
                list.Add(unit);
            }
        }

        return list;
    }

    /// <summary>
    /// Puts the unit in a random unoccupied square. Returns
    /// false if all squares are occupied.
    /// </summary>
    public bool PutUnit(Unit unit)
    {
        var square = GetRandomEmpty();
        if (!square.HasValue)
        {
            return false;
        }

        var old = PutUnit(unit, square.Value.Row, square.Value.Col);
        Debug.Assert(old == null, "Warning: unit not expected in square!");
        return true;
    }

    /// <summary>
    /// Puts the unit in Row/Column. Returns the unit that was in that
    /// square, or null if no unit was in that square.
    /// </summary>
    public Unit PutUnit(Unit unit, FormationRow row, FormationColumn column)
    {
        var rowIndex = (int)row;
        var columnIndex = (int)column;
        var current = _units[rowIndex, columnIndex];
        _units[rowIndex, columnIndex] = unit;
        unit.Formation = new FormationPair { Row = row, Col = column };
        return current;
    }

    public Unit GetUnit(FormationRow row, FormationColumn column)
    {
        var rowIndex = (int)row;
        var columnIndex = (int)column;
        return _units[rowIndex, columnIndex];
    }

    private FormationPair? GetRandomEmpty()
    {
        var randomSquares = AllSquares.ToList().GetShuffled();
        foreach (var square in randomSquares)
        {
            if (GetUnit(square.Row, square.Col) == null)
            {
                return square;
            }
        }

        return null;
    }
}
