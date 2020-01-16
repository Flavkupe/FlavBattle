
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

    public FormationPair(FormationRow row, FormationColumn col)
    {
        Row = row;
        Col = col;
    }

    public bool Equals(FormationPair other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override int GetHashCode()
    {
        return (int)Row * 10 + (int)Col;
    }
}

/// <summary>
/// Where, relative to view, is the front-middle
/// of the formation. That is, which direction
/// the units would be looking.
/// </summary>
public enum FormationOrientation
{
    BottomRight = 0,
    BottomLeft = 1,
}