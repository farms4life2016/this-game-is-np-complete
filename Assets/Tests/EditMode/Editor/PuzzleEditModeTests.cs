using System;
using System.Linq;
using NUnit.Framework;

public class PuzzleEditModeTests
{
    private static void PrintTransition(Puzzle before, Puzzle after, Block.Direction direction, Puzzle.TurnResult result)
    {
        TestContext.WriteLine($"Move: {direction}");
        TestContext.WriteLine("Before:");
        TestContext.WriteLine(before.ToString());
        TestContext.WriteLine("After:");
        TestContext.WriteLine(after.ToString());
        TestContext.WriteLine(
            $"Result: moved={result.AnyBlockMoved}, blocks={result.BlockCountBefore}->{result.BlockCountAfter}, status={result.GameOver}");
    }

    private static Puzzle.TurnResult RunTurn(Puzzle puzzle, Block.Direction direction)
    {
        Puzzle before = puzzle.Clone();
        Puzzle.TurnResult result = puzzle.Simulate(direction);
        PrintTransition(before, puzzle, direction, result);
        return result;
    }

    private static Block GetBlock(Puzzle puzzle, int x, int y)
    {
        return puzzle.Blocks.Single(block => block.X == x && block.Y == y);
    }

    /*
     * Rule:
     * All blocks move as far as possible in the chosen direction until the next tile is a wall.
     *
     * Initial (move Right):
     * y=0  B1  .  .  .  .  .
     * y=1  B2  .  .  #  .  .
     * y=2  .  B3  .  .  #  .
     * y=3  .  .  .  .  .  .
     *
     * Expected:
     * y=0  .  .  .  .  .  B1
     * y=1  .  .  B2 #  .  .
     * y=2  .  .  .  B3 #  .
     * y=3  .  .  .  .  .  .
     */
    [Test]
    public void Simulate_MovesUntilWall_VariedDistances()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(6, 4);
        tiles[3, 1] = Tile.Wall();
        tiles[4, 2] = Tile.Wall();

        var puzzle = new Puzzle(
            width: 6,
            height: 4,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 1, 2L),
                new Block(1, 2, 3L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 5, 0).Value, Is.EqualTo(1L));
        Assert.That(GetBlock(puzzle, 2, 1).Value, Is.EqualTo(2L));
        Assert.That(GetBlock(puzzle, 3, 2).Value, Is.EqualTo(3L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Entering a positive multiply tile applies multiplication exactly once.
     *
     * Initial (move Right):
     * y=0  B2  .  x3  .
     * y=1  B9  .   .  .
     *
     * Expected:
     * y=0  .  .  x3  B6
     * y=1  .  .   .  B9
     */
    [Test]
    public void Simulate_MultiplyPositive_ChangesValue()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[2, 0] = Tile.Multiply(3);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 2L),
                new Block(0, 1, 9L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(6L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Entering a negative multiply tile applies signed multiplication exactly once.
     *
     * Initial (move Right):
     * y=0  B3  x-2  .  .
     * y=1  B8   .   .  .
     *
     * Expected:
     * y=0  .  x-2  .  B-6
     * y=1  .   .   .   B8
     */
    [Test]
    public void Simulate_MultiplyNegative_ChangesValue()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Multiply(-2);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 3L),
                new Block(0, 1, 8L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(-6L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Entering a positive add tile applies addition exactly once.
     *
     * Initial (move Right):
     * y=0  B2  +5  .  .
     * y=1  B8   .  .  .
     *
     * Expected:
     * y=0  .  +5  .  B7
     * y=1  .   .  .  B8
     */
    [Test]
    public void Simulate_AddPositive_ChangesValue()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Add(5);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 2L),
                new Block(0, 1, 8L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(7L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Entering a negative add tile applies subtraction exactly once.
     *
     * Initial (move Right):
     * y=0  B10  -4  .  .
     * y=1   B8   .  .  .
     *
     * Expected:
     * y=0   .   -4  .  B6
     * y=1   .    .  .  B8
     */
    [Test]
    public void Simulate_AddNegative_ChangesValue()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Add(-4);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 10L),
                new Block(0, 1, 8L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(6L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Two positive blocks that end on one tile merge into one block whose value is their sum.
     *
     * Initial (move Up):
     * y=0  .  .  .
     * y=1  .  B2 .
     * y=2  .  B5 .
     *
     * Expected:
     * y=0  .  B7 .
     * y=1  .  .  .
     * y=2  .  .  .
     */
    [Test]
    public void Simulate_MergeTwoPositive_SumsValues()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(3, 3);
        var puzzle = new Puzzle(
            width: 3,
            height: 3,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(1, 1, 2L),
                new Block(1, 2, 5L)
            });

        RunTurn(puzzle, Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(7L));
    }

    /*
     * Rule:
     * Three or more blocks merge by summing all signed values.
     *
     * Initial (move Up):
     * y=0  .
     * y=1  B5
     * y=2  B-2
     * y=3  B7
     *
     * Expected:
     * y=0  B10
     * y=1  .
     * y=2  .
     * y=3  .
     */
    [Test]
    public void Simulate_MergeManyMixed_SumsValues()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 4);
        var puzzle = new Puzzle(
            width: 1,
            height: 4,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 1, 5L),
                new Block(0, 2, -2L),
                new Block(0, 3, 7L)
            });

        RunTurn(puzzle, Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(10L));
    }

    /*
     * Rule:
     * A block that stays on multiply after input receives multiply again on that turn.
     *
     * Initial:
     * y=0  B3  x2  #  .
     * y=1  B1   .  .  .
     *
     * Step 1 (Right) expected for B3: B6 on x2
     * Step 2 (Right) expected for B6: B12 stays on x2 due to wall
     */
    [Test]
    public void Simulate_MultiplyStayTile_ReappliesModifier()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Multiply(2);
        tiles[2, 0] = Tile.Wall();

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 3L),
                new Block(0, 1, 1L)
            });

        RunTurn(puzzle, Block.Direction.Right);
        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(6L));

        RunTurn(puzzle, Block.Direction.Right);
        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(12L));
    }

    /*
     * Rule:
     * A block that stays on add after input receives add again on that turn.
     *
     * Initial:
     * y=0  B3  +4  #  .
     * y=1  B1   .  .  .
     *
     * Step 1 (Right) expected for B3: B7 on +4
     * Step 2 (Right) expected for B7: B11 stays on +4 due to wall
     */
    [Test]
    public void Simulate_AddStayTile_ReappliesModifier()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Add(4);
        tiles[2, 0] = Tile.Wall();

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 3L),
                new Block(0, 1, 1L)
            });

        RunTurn(puzzle, Block.Direction.Right);
        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(7L));

        RunTurn(puzzle, Block.Direction.Right);
        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(11L));
    }

    /*
     * Rule:
     * Starting on multiply does not apply multiply before the turn starts.
     * Moving off multiply does not apply multiply on departure.
     *
     * Initial (move Right):
     * y=0  x3  .  .  .
     *      B2
     * y=1  B1  .  .  .
     *
     * Expected for top block: value remains 2 at destination.
     */
    [Test]
    public void Simulate_MultiplyStartTile_NoPreTurnApply()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[0, 0] = Tile.Multiply(3);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 2L),
                new Block(0, 1, 1L)
            });

        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(2L));

        RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(2L));
    }

    /*
     * Rule:
     * Starting on add does not apply add before the turn starts.
     * Moving off add does not apply add on departure.
     *
     * Initial (move Right):
     * y=0  +5  .  .  .
     *      B2
     * y=1  B1  .  .  .
     *
     * Expected for top block: value remains 2 at destination.
     */
    [Test]
    public void Simulate_AddStartTile_NoPreTurnApply()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[0, 0] = Tile.Add(5);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 2L),
                new Block(0, 1, 1L)
            });

        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(2L));

        RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(2L));
    }

    /*
     * Rule:
     * If multiple blocks enter one multiply tile, each block multiplies first, then merge sums results.
     *
     * Initial (move Up):
     * y=0  x2
     * y=1  B2
     * y=2  B3
     *
     * Expected:
     * Block values become 4 and 6 before merge.
     * Final merged block is B10 at y=0.
     */
    [Test]
    public void Simulate_MergeOnMultiply_ApplyBeforeMerge()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 3);
        tiles[0, 0] = Tile.Multiply(2);

        var puzzle = new Puzzle(
            width: 1,
            height: 3,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 1, 2L),
                new Block(0, 2, 3L)
            });

        RunTurn(puzzle, Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(10L));
    }

    /*
     * Rule:
     * Win occurs when one block remains and its value equals target.
     *
     * Initial (move Up): B1 above B2, target=3.
     * Expected: one block B3, status WIN.
     */
    [Test]
    public void CheckWinConditions_Win()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 2);
        var puzzle = new Puzzle(
            width: 1,
            height: 2,
            target: 3,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 1, 2L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Up);

        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
    }

    /*
     * Rule:
     * Lose occurs when one block remains and its value does not equal target.
     *
     * Initial (move Up): B1 above B2, target=4.
     * Expected: one block B3, status LOSE.
     */
    [Test]
    public void CheckWinConditions_Lose()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 2);
        var puzzle = new Puzzle(
            width: 1,
            height: 2,
            target: 4,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 1, 2L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Up);

        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.LOSE));
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.LOSE));
    }

    /*
     * Rule:
     * Status remains NOT_YET while two or more blocks remain.
     *
     * Initial (move Right):
     * y=0  B1  .
     * y=1   .  B2
     *
     * Expected:
     * y=0   .  B1
     * y=1   .  B2
     * Status NOT_YET.
     */
    [Test]
    public void CheckWinConditions_NotYet()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(2, 2);
        var puzzle = new Puzzle(
            width: 2,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(1, 1, 2L)
            });

        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
        Assert.That(puzzle.Blocks.Count, Is.EqualTo(2));
    }

    /*
     * Rule:
     * Out-of-bounds behaves like a wall, so edge blocks cannot move outside the board.
     *
     * Initial (move Right):
     * y=0  .  B5
     * y=1  B1  .
     *
     * Expected:
     * y=0  .  B5
     * y=1  .  B1
     */
    [Test]
    public void Simulate_BoundaryActsWall_StopsBlock()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(2, 2);
        var puzzle = new Puzzle(
            width: 2,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(1, 0, 5L),
                new Block(0, 1, 1L)
            });

        Puzzle.TurnResult result = RunTurn(puzzle, Block.Direction.Right);

        Assert.That(GetBlock(puzzle, 1, 0).Value, Is.EqualTo(5L));
        Assert.That(GetBlock(puzzle, 1, 1).Value, Is.EqualTo(1L));
        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Initial placement rejects blocks on wall tiles.
     *
     * Initial:
     * y=0  #  .
     * y=1  .  .
     * Block tries to spawn at (0,0).
     *
     * Expected:
     * Constructor throws ArgumentException.
     */
    [Test]
    public void Ctor_BlockOnWall_RejectsPlacement()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(2, 2);
        tiles[0, 0] = Tile.Wall();

        Assert.Throws<ArgumentException>(() =>
            new Puzzle(
                width: 2,
                height: 2,
                target: 99,
                tiles: tiles,
                blocks: new[]
                {
                    new Block(0, 0, 1L),
                    new Block(1, 1, 2L)
                }));
    }

    /*
     * Rule combo:
     * Path modifiers + merge + win across two turns.
     *
     * Initial:
     * y=0  B1  +2  x2  .
     * y=1  B3   .   .  .
     * target=9
     *
     * Step 1 Right:
     * y=0   .  +2  x2  B6
     * y=1   .   .   .  B3
     *
     * Step 2 Up:
     * y=0   .  +2  x2  B9
     * y=1   .   .   .   .
     * status WIN
     */
    [Test]
    public void Simulate_ComboPathModifiersMerge_Win()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(4, 2);
        tiles[1, 0] = Tile.Add(2);
        tiles[2, 0] = Tile.Multiply(2);

        var puzzle = new Puzzle(
            width: 4,
            height: 2,
            target: 9,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 1, 3L)
            });

        RunTurn(puzzle, Block.Direction.Right);
        Puzzle.TurnResult second = RunTurn(puzzle, Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 3, 0).Value, Is.EqualTo(9L));
        Assert.That(second.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
    }

    /*
     * Rule combo:
     * Negative add + negative multiply + three-block merge + loss across two turns.
     *
     * Initial:
     * y=0  B4  x-1  .
     * y=1  B3  -2   .
     * y=2   .   .  B5
     * target=10
     *
     * Step 1 Right:
     * y=0   .  x-1  B-4
     * y=1   .  -2   B1
     * y=2   .   .   B5
     *
     * Step 2 Up:
     * y=0   .  x-1  B2
     * y=1   .  -2   .
     * y=2   .   .   .
     * status LOSE
     */
    [Test]
    public void Simulate_ComboNegativeMathMerge_Lose()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(3, 3);
        tiles[1, 0] = Tile.Multiply(-1);
        tiles[1, 1] = Tile.Add(-2);

        var puzzle = new Puzzle(
            width: 3,
            height: 3,
            target: 10,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 4L),
                new Block(0, 1, 3L),
                new Block(2, 2, 5L)
            });

        RunTurn(puzzle, Block.Direction.Right);
        Puzzle.TurnResult second = RunTurn(puzzle, Block.Direction.Up);

        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 2, 0).Value, Is.EqualTo(2L));
        Assert.That(second.GameOver, Is.EqualTo(Puzzle.GameOverResult.LOSE));
    }

    /*
     * Rule:
     * Restoring a captured snapshot returns blocks to the exact pre-move state.
     *
     * Initial:
     * y=0  B2  .  .
     * y=1  B3  .  .
     *
     * Move Right:
     * y=0  .  .  B2
     * y=1  .  .  B3
     *
     * Restore snapshot:
     * y=0  B2  .  .
     * y=1  B3  .  .
     */
    [Test]
    public void RestoreDynamicState_RevertsToCapturedState()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(3, 2);
        var puzzle = new Puzzle(
            width: 3,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 2L),
                new Block(0, 1, 3L)
            });

        Puzzle.GameStateSnapshot snapshot = puzzle.CaptureDynamicState();
        RunTurn(puzzle, Block.Direction.Right);

        puzzle.RestoreDynamicState(snapshot);

        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(2L));
        Assert.That(GetBlock(puzzle, 0, 1).Value, Is.EqualTo(3L));
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
    }

    /*
     * Rule:
     * Restoring a snapshot also restores game-over status.
     *
     * Initial:
     * y=0  B1
     * y=1  B2
     * target=3
     *
     * Move Up -> WIN (B3)
     * Restore initial snapshot -> NOT_YET (B1 and B2)
     * Restore win snapshot -> WIN (B3)
     */
    [Test]
    public void RestoreDynamicState_RestoresGameStatus()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(1, 2);
        var puzzle = new Puzzle(
            width: 1,
            height: 2,
            target: 3,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 1, 2L)
            });

        Puzzle.GameStateSnapshot initialSnapshot = puzzle.CaptureDynamicState();
        RunTurn(puzzle, Block.Direction.Up);
        Puzzle.GameStateSnapshot winSnapshot = puzzle.CaptureDynamicState();

        puzzle.RestoreDynamicState(initialSnapshot);
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.NOT_YET));
        Assert.That(puzzle.Blocks.Count, Is.EqualTo(2));

        puzzle.RestoreDynamicState(winSnapshot);
        Assert.That(puzzle.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
        Assert.That(puzzle.Blocks.Count, Is.EqualTo(1));
        Assert.That(GetBlock(puzzle, 0, 0).Value, Is.EqualTo(3L));
    }

    /*
     * Rule:
     * Restoring an empty/default snapshot is rejected.
     */
    [Test]
    public void RestoreDynamicState_DefaultSnapshot_Throws()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(2, 2);
        var puzzle = new Puzzle(
            width: 2,
            height: 2,
            target: 99,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(1, 1, 2L)
            });

        Puzzle.GameStateSnapshot invalid = default;
        Assert.Throws<ArgumentException>(() => puzzle.RestoreDynamicState(invalid));
    }
}
