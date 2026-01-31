
using System.Collections.Generic;

public class Puzzle
{
    public int width;
    public int height;
    public long target;
    public EnvironmentTile[,] environmentTiles;
    public List<NumericTile> numericTiles; // unsorted arraylist: fast deletes but no order guarantees
    public GameOverResult gameOver = GameOverResult.NOT_YET;

    // simulates the puzzle for one step
    // assumes that the game is not over yet
    public void Simulate(NumericTile.Direction direction)
    {
        // set velocity of all numeric tiles
        foreach (NumericTile tile in numericTiles)
        {
            tile.velocity = direction;
        }

        // contract:
        // 1. all tiles to the left of finished (excluding finished itself) are done moving
        // 2. there are at least one numeric tile
        
        // move numeric tiles until we can't
        int finished = 0;
        while (finished < numericTiles.Count)
        {
            int i = finished;
            while (i <= numericTiles.Count)
            {
                int x = numericTiles[i].x;
                int y = numericTiles[i].y;
                NumericTile.Direction v = numericTiles[i].velocity;
                
                if (v == NumericTile.Direction.Up)
                {
                    // stop moving when hit wall
                    if (environmentTiles[x, y - 1].isWall)
                    {
                        numericTiles[i].velocity = NumericTile.Direction.None;
                        // swap with element at finished pos
                        if (i != finished)
                        {
                            (numericTiles[i], numericTiles[finished]) = (numericTiles[finished], numericTiles[i]);
                        }
                        finished++;

                    } else
                    {
                        // move onto tile and mark previous tile as unoccupied
                        environmentTiles[x, y].occupant = null;
                        numericTiles[i].y--;

                        // modify value if necessary
                        if (environmentTiles[x, y - 1].operation == EnvironmentTile.Operation.Add)
                        {
                            numericTiles[i].value += environmentTiles[x, y - 1].modifier;

                        } else if (environmentTiles[x, y - 1].operation == EnvironmentTile.Operation.Multiply)
                        {
                            numericTiles[i].value *= environmentTiles[x, y - 1].modifier;
                        }
                        // NOTE: MODIFICATION HAPPENS ON ARRIVAL, NOT ON DEPARTURE

                        // merge numeric tiles that occupy the same spot...
                        if (environmentTiles[x, y - 1].occupant == null)
                        {
                            environmentTiles[x, y - 1].occupant = numericTiles[i];
                            ++i;
                        } else
                        {
                            environmentTiles[x, y - 1].occupant.value += numericTiles[i].value;
                            // swap with tile at the end of the array and then delete the element at the end of the array.
                            // this gives us O(1) deletion but does not preserve ordering (which is not needed)
                            (numericTiles[i], numericTiles[^1]) = (numericTiles[^1], numericTiles[i]);
                            numericTiles.RemoveAt(numericTiles.Count - 1);
                        }
                        
                    }
                } // now handle the other cases....

            }
        }
    }

    public enum GameOverResult
    {
        WIN,
        LOSE,
        NOT_YET
    }

    public void CheckWinConditions()
    {
        if (numericTiles.Count == 1)
        {
            if (numericTiles[0].value == target)
            {
                gameOver = GameOverResult.WIN;
            } else
            {
                gameOver = GameOverResult.LOSE;
            }
        }
    }
}
