using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
#endif

/// <summary>
/// Deterministic simulation state for a single puzzle instance.
/// </summary>
public class Puzzle
{
    private readonly Tile[,] tiles;
    private readonly List<Block> blocks;
    private GameOverResult gameOver = GameOverResult.NOT_YET;

    /// <summary>
    /// Board width in tiles.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Board height in tiles.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Target value required for a win when one block remains.
    /// </summary>
    public long Target { get; }

    /// <summary>
    /// Current blocks on the board.
    /// </summary>
    public IReadOnlyList<Block> Blocks => blocks;

    /// <summary>
    /// Current puzzle status.
    /// </summary>
    public GameOverResult GameOver => gameOver;

    /// <summary>
    /// Creates a puzzle from a tile grid and starting blocks.
    /// </summary>
    public Puzzle(
        int width,
        int height,
        long target,
        Tile[,] tiles,
        IEnumerable<Block> blocks)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        if (tiles.GetLength(0) != width || tiles.GetLength(1) != height)
        {
            throw new ArgumentException("Tile dimensions must match width and height.", nameof(tiles));
        }

        if (blocks == null)
        {
            throw new ArgumentNullException(nameof(blocks));
        }

        Width = width;
        Height = height;
        Target = target;
        this.tiles = CloneTiles(tiles);
        this.blocks = CloneBlocks(blocks);

        if (this.blocks.Count == 0)
        {
            throw new ArgumentException("Puzzle must contain at least one block.", nameof(blocks));
        }

        ValidateInitialBlocks();
        CheckWinConditions();
    }

    /// <summary>
    /// Creates a grid initialized with blank tiles.
    /// </summary>
    public static Tile[,] CreateBlankGrid(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        var grid = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Tile.Blank();
            }
        }

        return grid;
    }

    /// <summary>
    /// Deep-copies the puzzle state.
    /// </summary>
    public Puzzle Clone()
    {
        var cloned = new Puzzle(Width, Height, Target, tiles, blocks.Select(block => block.Clone()))
        {
            gameOver = gameOver
        };
        return cloned;
    }

    /// <summary>
    /// Simulates one turn in the requested direction.
    /// </summary>
    /// <remarks>
    /// Rules implemented from <c>GameDesign.md</c>:
    /// block movement until wall, add/multiply on entry, add/multiply when blocked,
    /// then merge all blocks sharing a destination tile.
    /// </remarks>
    public TurnResult Simulate(Block.Direction direction)
    {
        if (gameOver != GameOverResult.NOT_YET)
        {
            throw new InvalidOperationException("Cannot simulate turns after game over.");
        }

        int blocksBefore = blocks.Count;
        bool anyBlockMoved = false;
        var movedBlocks = new List<Block>(blocks.Count);

        foreach (Block block in blocks)
        {
            (int x, int y, long value, bool moved) = ResolveMove(block, direction);
            anyBlockMoved |= moved;
            movedBlocks.Add(new Block(x, y, value));
        }

        blocks.Clear();

        // Merge step: all blocks that ended on the same tile collapse into one summed block.
        foreach (IGrouping<(int x, int y), Block> group in movedBlocks
                     .GroupBy(block => (block.X, block.Y))
                     .OrderBy(group => group.Key.Y)
                     .ThenBy(group => group.Key.X))
        {
            long mergedValue = 0;
            foreach (Block block in group)
            {
                mergedValue += block.Value;
            }

            blocks.Add(new Block(group.Key.x, group.Key.y, mergedValue));
        }

        CheckWinConditions();
        return new TurnResult(direction, anyBlockMoved, blocksBefore, blocks.Count, gameOver);
    }

    /// <summary>
    /// Gets the tile at a board coordinate.
    /// </summary>
    public Tile GetTile(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            throw new ArgumentOutOfRangeException($"({x}, {y}) is out of bounds.");
        }

        return tiles[x, y];
    }

    /// <summary>
    /// Updates <see cref="GameOver"/> according to current block state.
    /// </summary>
    public void CheckWinConditions()
    {
        if (blocks.Count == 1)
        {
            gameOver = blocks[0].Value == Target
                ? GameOverResult.WIN
                : GameOverResult.LOSE;
            return;
        }

        gameOver = GameOverResult.NOT_YET;
    }

    private (int x, int y, long value, bool moved) ResolveMove(Block block, Block.Direction direction)
    {
        (int deltaX, int deltaY) = GetDirectionDelta(direction);
        int x = block.X;
        int y = block.Y;
        long value = block.Value;
        bool moved = false;

        while (true)
        {
            int nextX = x + deltaX;
            int nextY = y + deltaY;

            if (IsWall(nextX, nextY))
            {
                break;
            }

            x = nextX;
            y = nextY;
            moved = true;
            value = tiles[x, y].ApplyModifier(value);
        }

        if (!moved)
        {
            // Important rule: if the block cannot move, its current tile still applies once.
            value = tiles[x, y].ApplyModifier(value);
        }

        return (x, y, value, moved);
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    private bool IsWall(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            // Infinite border wall rule from the design doc.
            return true;
        }

        return tiles[x, y].IsWall;
    }

    private static (int deltaX, int deltaY) GetDirectionDelta(Block.Direction direction)
    {
        return direction switch
        {
            Block.Direction.Up => (0, -1),
            Block.Direction.Down => (0, 1),
            Block.Direction.Left => (-1, 0),
            Block.Direction.Right => (1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    private Tile[,] CloneTiles(Tile[,] sourceTiles)
    {
        var result = new Tile[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = sourceTiles[x, y];
                if (tile == null)
                {
                    throw new ArgumentException($"Tile at ({x}, {y}) is null.", nameof(sourceTiles));
                }

                result[x, y] = tile;
            }
        }

        return result;
    }

    private static List<Block> CloneBlocks(IEnumerable<Block> sourceBlocks)
    {
        var result = new List<Block>();
        foreach (Block block in sourceBlocks)
        {
            if (block == null)
            {
                throw new ArgumentException("Block list cannot contain null items.", nameof(sourceBlocks));
            }

            result.Add(block.Clone());
        }

        return result;
    }

    private void ValidateInitialBlocks()
    {
        var positions = new HashSet<(int x, int y)>();
        foreach (Block block in blocks)
        {
            if (!IsInsideBoard(block.X, block.Y))
            {
                throw new ArgumentException(
                    $"Block at ({block.X}, {block.Y}) is out of bounds.");
            }

            if (tiles[block.X, block.Y].IsWall)
            {
                throw new ArgumentException(
                    $"Block at ({block.X}, {block.Y}) cannot be placed on a wall tile.");
            }

            if (!positions.Add((block.X, block.Y)))
            {
                throw new ArgumentException(
                    $"Duplicate block position detected at ({block.X}, {block.Y}).");
            }
        }
    }

    public enum GameOverResult
    {
        WIN,
        LOSE,
        NOT_YET
    }

    /// <summary>
    /// Metadata about one simulated turn.
    /// </summary>
    public readonly struct TurnResult
    {
        public Block.Direction Direction { get; }
        public bool AnyBlockMoved { get; }
        public int BlockCountBefore { get; }
        public int BlockCountAfter { get; }
        public GameOverResult GameOver { get; }
        public bool IsGameOver => GameOver != GameOverResult.NOT_YET;
        public bool IsWin => GameOver == GameOverResult.WIN;

        public TurnResult(
            Block.Direction direction,
            bool anyBlockMoved,
            int blockCountBefore,
            int blockCountAfter,
            GameOverResult gameOver)
        {
            Direction = direction;
            AnyBlockMoved = anyBlockMoved;
            BlockCountBefore = blockCountBefore;
            BlockCountAfter = blockCountAfter;
            GameOver = gameOver;
        }
    }
}

#if UNITY_INCLUDE_TESTS
public class PuzzleTests
{
    [Test]
    public void Simulate_AppliesModifiersOnEntryAndWhenBlocked()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 2);
        tiles[0, 0] = Tile.Add(2);

        var puzzle = new Puzzle(
            width: 1,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 3),
                new Block(0, 1, 4)
            });

        puzzle.Simulate(Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(puzzle.Blocks[0].X, Is.EqualTo(0));
        Assert.That(puzzle.Blocks[0].Y, Is.EqualTo(0));
        Assert.That(puzzle.Blocks[0].Value, Is.EqualTo(11));
    }

    [Test]
    public void Simulate_AppliesAllPathModifiers()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Add(2);
        tiles[2, 0] = Tile.Multiply(3);
        tiles[3, 0] = Tile.Add(1);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1),
                new Block(0, 1, 0)
            });

        puzzle.Simulate(Block.Direction.Right);

        Block topRowBlock = puzzle.Blocks.Single(block => block.Y == 0);
        Assert.That(topRowBlock.X, Is.EqualTo(3));
        Assert.That(topRowBlock.Value, Is.EqualTo(10));
    }

    [Test]
    public void Simulate_MergesBlocksWithSameDestination()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(3, 3);
        var puzzle = new Puzzle(
            width: 3,
            height: 3,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(1, 1, 2),
                new Block(1, 2, 5)
            });

        puzzle.Simulate(Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(puzzle.Blocks[0].X, Is.EqualTo(1));
        Assert.That(puzzle.Blocks[0].Y, Is.EqualTo(0));
        Assert.That(puzzle.Blocks[0].Value, Is.EqualTo(7));
    }

    [Test]
    public void Simulate_ReturnsWinWhenSingleBlockMatchesTarget()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 2);
        var puzzle = new Puzzle(
            width: 1,
            height: 2,
            target: 3,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1),
                new Block(0, 1, 2)
            });

        Puzzle.TurnResult result = puzzle.Simulate(Block.Direction.Up);

        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
        Assert.That(result.IsWin, Is.True);
    }
}
#endif
