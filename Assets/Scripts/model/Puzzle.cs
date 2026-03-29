
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
        // set velocity of all numeric tiles (except frozen ones)
        foreach (NumericTile tile in numericTiles)
        {
            if (tile.statusEffect != NumericTile.StatusEffect.Frozen)
            {
                tile.direction = direction;
            }
            
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
                NumericTile.Direction v = numericTiles[i].direction;
                
                if (v == NumericTile.Direction.Up)
                {
                    // stop moving when hit wall
                    if (environmentTiles[x, y - 1].isWall)
                    {
                        numericTiles[i].direction = NumericTile.Direction.None;
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

                        // NOTE: MODIFICATION HAPPENS ON ARRIVAL, NOT ON DEPARTURE
                        // modify value if necessary
                        if (environmentTiles[x, y - 1].operation == EnvironmentTile.Operation.Add)
                        {
                            numericTiles[i].value += environmentTiles[x, y - 1].modifier;

                        } else if (environmentTiles[x, y - 1].operation == EnvironmentTile.Operation.Multiply)
                        {
                            numericTiles[i].value *= environmentTiles[x, y - 1].modifier;
                        }
                        // apply status effects
                        if (environmentTiles[x, y - 1].trap == EnvironmentTile.Trap.Soapy)
                        {
                            // soap will clean sticky (and put out fires?), consuming the soap
                            if (numericTiles[i].statusEffect == NumericTile.StatusEffect.Sticky || numericTiles[i].statusEffect == NumericTile.StatusEffect.OnFire)
                            {
                                numericTiles[i].statusEffect = NumericTile.StatusEffect.None;
                                environmentTiles[x, y - 1].trap = EnvironmentTile.Trap.None;
                            }
                            // if no status, then soap is not consumed
                            
                        } else if (environmentTiles[x, y - 1].trap == EnvironmentTile.Trap.Sappy)
                        {
                            // sap stops movement and applies sticky
                            if (numericTiles[i].statusEffect == NumericTile.StatusEffect.None)
                            {
                                numericTiles[i].statusEffect = NumericTile.StatusEffect.Sticky;
                                // TODO: duplicated movement stopping code...actually we should move this code to the bottom of the simulate function to catch all sticky
                                numericTiles[i].direction = NumericTile.Direction.None;
                                // swap with element at finished pos
                                if (i != finished)
                                {
                                    (numericTiles[i], numericTiles[finished]) = (numericTiles[finished], numericTiles[i]);
                                }
                                finished++;
                                // sap is not consumed
                            } else if (numericTiles[i].statusEffect == NumericTile.StatusEffect.OnFire)
                            {
                                // onFire tiles stop on sap but destroy the sap trap permanently 
                                // TODO: duplicated movement stopping code...
                                numericTiles[i].direction = NumericTile.Direction.None;
                                // swap with element at finished pos
                                if (i != finished)
                                {
                                    (numericTiles[i], numericTiles[finished]) = (numericTiles[finished], numericTiles[i]);
                                }
                                finished++;
                                environmentTiles[x, y - 1].trap = EnvironmentTile.Trap.None;
                            }
                            // if already sticky, then sap is not consumed

                        } else if (environmentTiles[x, y - 1].trap == EnvironmentTile.Trap.Fiery)
                        {
                            // fiery tiles light up their occupants and preserve movement
                            // the fire burns away the sap and overrides it (so Fiery is just better soap???)
                            if (numericTiles[i].statusEffect == NumericTile.StatusEffect.None || numericTiles[i].statusEffect == NumericTile.StatusEffect.Sticky)
                            {
                                numericTiles[i].statusEffect = NumericTile.StatusEffect.OnFire;
                                // possible bug/feature (things you should account for): should previously sticky tiles should lose their direction upon reaching this tile?
                            }

                        } else if (environmentTiles[x, y - 1].trap == EnvironmentTile.Trap.Snowy)
                        {
                            // snowy tiles will freeze its occupant and convert it into a temporary wall.
                            // frozen overrides sticky, and frozen cancels out if tile is already on fire (but still stops movement)
                            if (numericTiles[i].statusEffect == NumericTile.StatusEffect.None || numericTiles[i].statusEffect == NumericTile.StatusEffect.Sticky)
                            {
                                numericTiles[i].statusEffect = NumericTile.StatusEffect.Frozen;
                                // TODO: duplicated movement stopping code...actually we should move this code to the bottom of the simulate function to catch all sticky
                                numericTiles[i].direction = NumericTile.Direction.None;
                                // swap with element at finished pos
                                if (i != finished)
                                {
                                    (numericTiles[i], numericTiles[finished]) = (numericTiles[finished], numericTiles[i]);
                                }
                                finished++;
                                // consume snowy trap
                                environmentTiles[x, y - 1].trap = EnvironmentTile.Trap.None;

                            } else if (numericTiles[i].statusEffect == NumericTile.StatusEffect.OnFire)
                            {
                                numericTiles[i].statusEffect = NumericTile.StatusEffect.None;
                                // TODO: duplicated movement stopping code...actually we should move this code to the bottom of the simulate function to catch all sticky
                                numericTiles[i].direction = NumericTile.Direction.None;
                                // swap with element at finished pos
                                if (i != finished)
                                {
                                    (numericTiles[i], numericTiles[finished]) = (numericTiles[finished], numericTiles[i]);
                                }
                                finished++;
                                // consume snowy trap
                                environmentTiles[x, y - 1].trap = EnvironmentTile.Trap.None;
                            }
                        }

                        // merge numeric tiles that occupy the same spot...
                        if (environmentTiles[x, y - 1].occupant == null)
                        {
                            environmentTiles[x, y - 1].occupant = numericTiles[i];
                            ++i;
                        } else
                        {
                            // merge values
                            environmentTiles[x, y - 1].occupant.value += numericTiles[i].value;

                            // merge direction and status effects

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
