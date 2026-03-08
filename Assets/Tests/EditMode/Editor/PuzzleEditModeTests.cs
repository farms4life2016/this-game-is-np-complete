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
                new Block(0, 0, 3L),
                new Block(0, 1, 4L)
            });

        Puzzle before = puzzle.Clone();
        Puzzle.TurnResult result = puzzle.Simulate(Block.Direction.Up);
        PrintTransition(before, puzzle, Block.Direction.Up, result);

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
                new Block(0, 0, 1L),
                new Block(0, 1, 0L)
            });

        Puzzle before = puzzle.Clone();
        Puzzle.TurnResult result = puzzle.Simulate(Block.Direction.Right);
        PrintTransition(before, puzzle, Block.Direction.Right, result);

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
                new Block(1, 1, 2L),
                new Block(1, 2, 5L)
            });

        Puzzle before = puzzle.Clone();
        Puzzle.TurnResult result = puzzle.Simulate(Block.Direction.Up);
        PrintTransition(before, puzzle, Block.Direction.Up, result);

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
                new Block(0, 0, 1L),
                new Block(0, 1, 2L)
            });

        Puzzle before = puzzle.Clone();
        Puzzle.TurnResult result = puzzle.Simulate(Block.Direction.Up);
        PrintTransition(before, puzzle, Block.Direction.Up, result);

        Assert.That(result.GameOver, Is.EqualTo(Puzzle.GameOverResult.WIN));
        Assert.That(result.IsWin, Is.True);
    }
}
