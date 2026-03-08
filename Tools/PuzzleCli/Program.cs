using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("NP-Complete Puzzle CLI");
        Console.WriteLine("Controls: w/a/s/d + Enter, r = reset puzzle, q = quit.");
        Console.WriteLine();

        Puzzle puzzle = CreateSamplePuzzle();

        while (true)
        {
            Console.WriteLine(puzzle.ToString());

            if (puzzle.GameOver != Puzzle.GameOverResult.NOT_YET)
            {
                Console.WriteLine($"Game over: {puzzle.GameOver}");
            }

            Console.Write("Command (w/a/s/d, r, q): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine();
                continue;
            }

            char command = char.ToLowerInvariant(input.Trim()[0]);
            if (command == 'q')
            {
                break;
            }

            if (command == 'r')
            {
                puzzle = CreateSamplePuzzle();
                Console.WriteLine("Puzzle reset.");
                Console.WriteLine();
                continue;
            }

            if (!TryParseDirection(command, out Block.Direction direction))
            {
                Console.WriteLine("Invalid command. Use w/a/s/d, r, or q.");
                Console.WriteLine();
                continue;
            }

            if (puzzle.GameOver != Puzzle.GameOverResult.NOT_YET)
            {
                Console.WriteLine("Puzzle is already over. Press r to reset or q to quit.");
                Console.WriteLine();
                continue;
            }

            Puzzle before = puzzle.Clone();
            Puzzle.TurnResult result = puzzle.Simulate(direction);

            Console.WriteLine($"Move: {direction}");
            Console.WriteLine("Before:");
            Console.WriteLine(before.ToString());
            Console.WriteLine("After:");
            Console.WriteLine(puzzle.ToString());
            Console.WriteLine(
                $"Result: moved={result.AnyBlockMoved}, blocks={result.BlockCountBefore}->{result.BlockCountAfter}, status={result.GameOver}");
            Console.WriteLine();
        }
    }

    private static bool TryParseDirection(char command, out Block.Direction direction)
    {
        switch (command)
        {
            case 'w':
                direction = Block.Direction.Up;
                return true;
            case 'a':
                direction = Block.Direction.Left;
                return true;
            case 's':
                direction = Block.Direction.Down;
                return true;
            case 'd':
                direction = Block.Direction.Right;
                return true;
            default:
                direction = default;
                return false;
        }
    }

    private static Puzzle CreateSamplePuzzle()
    {
        Tile[,] tiles = Puzzle.CreateBlankGrid(6, 5);
        tiles[1, 0] = Tile.Add(2);
        tiles[2, 0] = Tile.Multiply(2);
        tiles[4, 1] = Tile.Wall();
        tiles[4, 2] = Tile.Wall();
        tiles[3, 3] = Tile.Add(3);
        tiles[1, 4] = Tile.Multiply(3);

        return new Puzzle(
            width: 6,
            height: 5,
            target: 20,
            tiles: tiles,
            blocks: new[]
            {
                new Block(0, 0, 1L),
                new Block(0, 4, 2L),
                new Block(5, 3, 3L)
            });
    }
}
