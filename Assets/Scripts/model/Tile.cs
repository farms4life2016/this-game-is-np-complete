using System;

/// <summary>
/// Immutable model for a board tile in the simulation layer.
/// </summary>
public class Tile
{
    /// <summary>
    /// Gets the tile kind (blank, add, multiply, wall).
    /// </summary>
    public TileType Type { get; }

    /// <summary>
    /// Gets the tile modifier value used by add/multiply tiles.
    /// </summary>
    public long Modifier { get; }

    /// <summary>
    /// True when this tile is a wall.
    /// </summary>
    public bool IsWall => Type == TileType.Wall;

    /// <summary>
    /// Creates a new simulation tile.
    /// </summary>
    public Tile(TileType type, long modifier = 0)
    {
        Type = type;
        Modifier = modifier;
    }

    /// <summary>
    /// Applies this tile's modifier to a block value.
    /// </summary>
    /// <remarks>
    /// Blank returns the value unchanged.
    /// Wall is invalid here because blocks cannot occupy walls.
    /// </remarks>
    public long ApplyModifier(long value)
    {
        return Type switch
        {
            TileType.Add => value + Modifier,
            TileType.Multiply => value * Modifier,
            TileType.Blank => value,
            TileType.Wall => throw new InvalidOperationException("Cannot apply modifiers on a wall tile."),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Tile Blank() => new(TileType.Blank, 0);
    public static Tile Add(long modifier) => new(TileType.Add, modifier);
    public static Tile Multiply(long modifier) => new(TileType.Multiply, modifier);
    public static Tile Wall() => new(TileType.Wall, 0);
}

/// <summary>
/// Tile categories used by the puzzle simulation.
/// </summary>
public enum TileType
{
    Blank,
    Add,
    Multiply,
    Wall
}
