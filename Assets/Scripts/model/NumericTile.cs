
/**
 * Represents tiles that would get moved when u give a directional input
 */
public class NumericTile
{
    // should make some fields private to avoid unwanted mutations!!!
    public long value;
    public int x;
    public int y;
    public Direction direction;
    public StatusEffect statusEffect;
    
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public enum StatusEffect
    {
        Frozen,
        OnFire,
        Sticky,
        None
    }
}
