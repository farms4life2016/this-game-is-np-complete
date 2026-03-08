/// <summary>
/// Mutable model for a movable puzzle block in the simulation layer.
/// </summary>
public class Block
{
    /// <summary>
    /// Current numeric value for the block.
    /// </summary>
    public long Value { get; private set; }

    /// <summary>
    /// Current X position on the board grid.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Current Y position on the board grid.
    /// </summary>
    public int Y { get; private set; }

    public Block(int x, int y, long value)
    {
        X = x;
        Y = y;
        Value = value;
    }

    /// <summary>
    /// Updates block position in grid coordinates.
    /// </summary>
    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Replaces the current block value.
    /// </summary>
    public void SetValue(long value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a deep copy of this block.
    /// </summary>
    public Block Clone()
    {
        return new Block(X, Y, Value);
    }

    /// <summary>
    /// Cardinal directions accepted by the turn simulation.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
