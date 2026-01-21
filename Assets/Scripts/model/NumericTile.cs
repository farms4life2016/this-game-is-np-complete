
/**
 * Represents tiles that would get moved when u give a directional input
 */
public class NumericTile
{
    public int value; // should make some fields private to avoid unwanted mutations!!!
    public int x;
    public int y;
    public Direction velocity;
    
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }
}
