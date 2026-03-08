using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        ValidateBlocks(this.blocks, nameof(blocks));
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
    /// Captures only the dynamic puzzle state for undo/redo.
    /// </summary>
    public GameStateSnapshot CaptureDynamicState()
    {
        var blockStates = new BlockState[blocks.Count];
        for (int i = 0; i < blocks.Count; i++)
        {
            Block block = blocks[i];
            blockStates[i] = new BlockState(block.X, block.Y, block.Value);
        }

        return new GameStateSnapshot(blockStates, gameOver);
    }

    /// <summary>
    /// Restores a previously captured dynamic puzzle state.
    /// </summary>
    public void RestoreDynamicState(GameStateSnapshot snapshot)
    {
        if (snapshot.BlockCount <= 0)
        {
            throw new ArgumentException("Snapshot must contain at least one block.", nameof(snapshot));
        }

        var restoredBlocks = new List<Block>(snapshot.BlockCount);
        foreach (BlockState blockState in snapshot.Blocks)
        {
            restoredBlocks.Add(new Block(blockState.X, blockState.Y, blockState.Value));
        }

        ValidateBlocks(restoredBlocks, nameof(snapshot));

        GameOverResult computed = EvaluateGameOver(restoredBlocks);
        if (computed != snapshot.GameOver)
        {
            throw new ArgumentException(
                $"Snapshot status {snapshot.GameOver} does not match computed status {computed}.",
                nameof(snapshot));
        }

        blocks.Clear();
        blocks.AddRange(restoredBlocks);
        gameOver = snapshot.GameOver;
    }

    /// <summary>
    /// Returns an ASCII snapshot of the current puzzle state for debugging.
    /// </summary>
    public override string ToString()
    {
        var blocksByPosition = new Dictionary<(int x, int y), Block>();
        foreach (Block block in blocks)
        {
            blocksByPosition[(block.X, block.Y)] = block;
        }

        string[,] cells = new string[Width, Height];
        int cellWidth = 3;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                string cellText = blocksByPosition.TryGetValue((x, y), out Block block)
                    ? $"B{block.Value}"
                    : FormatTileDebugValue(tiles[x, y]);

                cells[x, y] = cellText;
                if (cellText.Length > cellWidth)
                {
                    cellWidth = cellText.Length;
                }
            }
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Target={Target} | Blocks={blocks.Count} | Status={gameOver}");
        builder.AppendLine("Legend: Bn=block value n, #=wall, +n=add, xn=multiply, .=blank");
        builder.Append("    ");
        for (int x = 0; x < Width; x++)
        {
            builder.Append(x.ToString().PadLeft(cellWidth));
            builder.Append(' ');
        }

        builder.AppendLine();
        for (int y = 0; y < Height; y++)
        {
            builder.Append(y.ToString().PadLeft(3));
            builder.Append(' ');
            for (int x = 0; x < Width; x++)
            {
                builder.Append(cells[x, y].PadLeft(cellWidth));
                builder.Append(' ');
            }

            builder.AppendLine();
        }

        return builder.ToString();
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
        gameOver = EvaluateGameOver(blocks);
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

    private void ValidateBlocks(IReadOnlyList<Block> blocksToValidate, string paramName)
    {
        var positions = new HashSet<(int x, int y)>();
        foreach (Block block in blocksToValidate)
        {
            if (!IsInsideBoard(block.X, block.Y))
            {
                throw new ArgumentException(
                    $"Block at ({block.X}, {block.Y}) is out of bounds.",
                    paramName);
            }

            if (tiles[block.X, block.Y].IsWall)
            {
                throw new ArgumentException(
                    $"Block at ({block.X}, {block.Y}) cannot be placed on a wall tile.",
                    paramName);
            }

            if (!positions.Add((block.X, block.Y)))
            {
                throw new ArgumentException(
                    $"Duplicate block position detected at ({block.X}, {block.Y}).",
                    paramName);
            }
        }
    }

    private GameOverResult EvaluateGameOver(IReadOnlyList<Block> blocksForState)
    {
        if (blocksForState.Count == 1)
        {
            return blocksForState[0].Value == Target
                ? GameOverResult.WIN
                : GameOverResult.LOSE;
        }

        return GameOverResult.NOT_YET;
    }

    private static string FormatTileDebugValue(Tile tile)
    {
        return tile.Type switch
        {
            TileType.Blank => ".",
            TileType.Wall => "#",
            TileType.Add => tile.Modifier >= 0 ? $"+{tile.Modifier}" : tile.Modifier.ToString(),
            TileType.Multiply => $"x{tile.Modifier}",
            _ => "?"
        };
    }

    public enum GameOverResult
    {
        WIN,
        LOSE,
        NOT_YET
    }

    /// <summary>
    /// Immutable block payload used in dynamic state snapshots.
    /// </summary>
    public readonly struct BlockState
    {
        public int X { get; }
        public int Y { get; }
        public long Value { get; }

        public BlockState(int x, int y, long value)
        {
            X = x;
            Y = y;
            Value = value;
        }
    }

    /// <summary>
    /// Dynamic state payload for undo/redo.
    /// </summary>
    public readonly struct GameStateSnapshot
    {
        private readonly BlockState[] blockStates;

        internal GameStateSnapshot(BlockState[] blockStates, GameOverResult gameOver)
        {
            this.blockStates = blockStates ?? throw new ArgumentNullException(nameof(blockStates));
            GameOver = gameOver;
        }

        public IReadOnlyList<BlockState> Blocks => blockStates ?? Array.Empty<BlockState>();
        public int BlockCount => blockStates?.Length ?? 0;
        public GameOverResult GameOver { get; }
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
