

public class Puzzle
{
    public int width;
    public int height;
    public EnvironmentTile[,] environmentTiles;
    public NumericTile[] numericTiles;
    public int numericArrayEnd; // BAD CODE: should wrap in a custom object or use a list!

    // simulates the puzzle for one step
    public void Simulate(NumericTile.Direction direction)
    {
        // set velocity of all numeric tiles
        foreach (NumericTile tile in numericTiles)
        {
            tile.velocity = direction;
        }

        // contract:
        // 1. all tiles to the left of finished (excluding finished itself) are done moving
        // 2. all tiles to the right of numericArrayEnd (excluding numericArrayEnd itself) are deleted
        
        // move numeric tiles until we can't
        int finished = 0;
        while (finished <= numericArrayEnd)
        {
            int i = finished;
            while (i <= numericArrayEnd)
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
                            // swap with tile at the end of the array
                            (numericTiles[i], numericTiles[numericArrayEnd]) = (numericTiles[numericArrayEnd], numericTiles[i]);
                            --numericArrayEnd;
                        }
                        
                    }
                } // now handle the other cases....

            }
        }
    }
}
