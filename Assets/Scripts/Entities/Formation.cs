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

    public List<Unit> GetUnits()
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

    public List<Unit> GetUnits(IEnumerable<FormationPair> pairs)
    {
        var list = new List<Unit>();
        foreach (var pair in pairs)
        {
            var unit = GetUnit(pair.Row, pair.Col);
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
        if (unit == null)
        {
            return false;
        }

        var square = GetRandomEmpty();
        if (!square.HasValue)
        {
            return false;
        }

        var old = PutUnit(unit, square.Value.Row, square.Value.Col);
        Debug.Assert(old == null, "Warning: unit not expected in square!");
        return true;
    }

    public Unit PutUnit(Unit unit, FormationPair pair)
    {
        return PutUnit(unit, pair.Row, pair.Col);
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
        if (unit != null)
        {
            unit.Formation = new FormationPair { Row = row, Col = column };
            unit.IsInFormation = true;
        }
        else if (current != null)
        {
            // Unit replaced with null; it is no longer in formation
            current.IsInFormation = false;
        }

        return current;
    }

    public void RemoveUnit(Unit e)
    {
        Debug.Assert(e.IsInFormation, "Attempting to remove unit not flagged for formation!");
        var row = e.Formation.Row;
        var col = e.Formation.Col;
        var current = PutUnit(null, row, col);
        Debug.Assert(e == current, "Attempting to remove unit not in this formation!");
    }

    public Unit GetUnit(FormationRow row, FormationColumn column)
    {
        var rowIndex = (int)row;
        var columnIndex = (int)column;
        return _units[rowIndex, columnIndex];
    }

    /// <summary>
    /// Puts unit in destination. If there was something
    /// in destination, that thing is swapped with the current unit.
    /// Returns whatever was in the destination, or null if nothing
    /// was there.
    /// </summary>
    public Unit MoveUnit(Unit unit, FormationPair destination)
    {
        var fromFormation = unit.IsInFormation;
        var source = unit.Formation;
        var current = PutUnit(unit, destination);

        // If not in formation, do not swap
        if (fromFormation)
        {
            // Make room for incoming swap first
            _units[(int)source.Row, (int)source.Col] = null;

            // Note: even if current is null, this will do the right thing
            PutUnit(current, source);
        }
        else if (current != null)
        {
            current.IsInFormation = false;
        }

        return current;
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
