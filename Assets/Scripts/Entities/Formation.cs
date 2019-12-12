using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FormationInfo
{
    public FormationPair FormationPair;
    public Unit Unit;
}

public class Formation
{
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

    public IEnumerable<FormationPair> GetOccupiedPositions()
    {
        return FormationUtils.AllSquares.ToList().Where(a => GetUnit(a.Row, a.Col) != null);
    }

    /// <summary>
    /// Gets all info associated with occupied positions
    /// </summary>
    public IEnumerable<FormationInfo> GetOccupiedPositionInfo()
    {
        return GetOccupiedPositions().Select(a => new FormationInfo
        {
            FormationPair = a,
            Unit = GetUnit(a.Row, a.Col)
        });
    }

    private FormationPair? GetRandomEmpty()
    {
        var randomSquares = FormationUtils.AllSquares.ToList().GetShuffled();
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
