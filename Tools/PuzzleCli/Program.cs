using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("NP-Complete Puzzle CLI");
        Console.WriteLine("Controls: w/a/s/d + Enter, u = undo, y = redo, r = reset puzzle, q = quit.");
        Console.WriteLine();

        Puzzle puzzle = CreateSamplePuzzle();
        var undoStack = new Stack<Puzzle.GameStateSnapshot>();
        var redoStack = new Stack<Puzzle.GameStateSnapshot>();

        while (true)
        {
            Console.WriteLine(puzzle.ToString());
            Console.WriteLine($"History: undo={undoStack.Count}, redo={redoStack.Count}");

            if (puzzle.GameOver != Puzzle.GameOverResult.NOT_YET)
            {
                Console.WriteLine($"Game over: {puzzle.GameOver}");
            }

            Console.Write("Command (w/a/s/d, u, y, r, q): ");
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
                undoStack.Clear();
                redoStack.Clear();
                Console.WriteLine("Puzzle reset.");
                Console.WriteLine();
                continue;
            }

            if (command == 'u')
            {
                if (undoStack.Count == 0)
                {
                    Console.WriteLine("Nothing to undo.");
                    Console.WriteLine();
                    continue;
                }

                Puzzle beforeUndo = puzzle.Clone();
                redoStack.Push(puzzle.CaptureDynamicState());
                puzzle.RestoreDynamicState(undoStack.Pop());
                Console.WriteLine("Undo:");
                Console.WriteLine("Before:");
                Console.WriteLine(beforeUndo.ToString());
                Console.WriteLine("After:");
                Console.WriteLine(puzzle.ToString());
                Console.WriteLine();
                continue;
            }

            if (command == 'y')
            {
                if (redoStack.Count == 0)
                {
                    Console.WriteLine("Nothing to redo.");
                    Console.WriteLine();
                    continue;
                }

                Puzzle beforeRedo = puzzle.Clone();
                undoStack.Push(puzzle.CaptureDynamicState());
                puzzle.RestoreDynamicState(redoStack.Pop());
                Console.WriteLine("Redo:");
                Console.WriteLine("Before:");
                Console.WriteLine(beforeRedo.ToString());
                Console.WriteLine("After:");
                Console.WriteLine(puzzle.ToString());
                Console.WriteLine();
                continue;
            }

            if (!TryParseDirection(command, out Block.Direction direction))
            {
                Console.WriteLine("Invalid command. Use w/a/s/d, u, y, r, or q.");
                Console.WriteLine();
                continue;
            }

            if (puzzle.GameOver != Puzzle.GameOverResult.NOT_YET)
            {
                Console.WriteLine("Puzzle is already over. Press r to reset or q to quit.");
                Console.WriteLine();
                continue;
            }

            undoStack.Push(puzzle.CaptureDynamicState());
            Puzzle beforeMove = puzzle.Clone();
            Puzzle.TurnResult result = puzzle.Simulate(direction);
            redoStack.Clear();

            Console.WriteLine($"Move: {direction}");
            Console.WriteLine("Before:");
            Console.WriteLine(beforeMove.ToString());
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
