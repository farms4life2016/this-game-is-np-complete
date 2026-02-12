
/**
 * Represents tiles in the environment that cannot be moved.
 * E.g. walls, open spaces, multiply by, add by, etc.
 */
public class EnvironmentTile
{
    // consider making all fields readonly!!!

    public bool isWall;
    public Operation operation;
    public Trap trap;
    public long modifier; // the number added or multiplied by
    public NumericTile occupant;

    public enum Operation 
    {
        Add,
        Multiply,
        None
    }

    public enum Trap
    {
        Snowy,
        Fiery,
        Sappy,
        Soapy,
        None
    }

}
