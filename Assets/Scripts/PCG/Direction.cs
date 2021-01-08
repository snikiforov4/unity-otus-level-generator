public class Direction
{
    public static readonly Direction Left  = new Direction("Left", 1);  // 0001
    public static readonly Direction Right = new Direction("Right", 2); // 0010
    public static readonly Direction Up    = new Direction("Up", 4);    // 0100
    public static readonly Direction Down  = new Direction("Down", 8);  // 1000

    public static readonly Direction[] All = {Left, Right, Up, Down};

    public int Bitmask => m_Bitmask;

    private readonly string m_Name;
    private readonly int m_Bitmask;

    private Direction(string name, int bitmask)
    {
        m_Bitmask = bitmask;
        m_Name = name;
    }

    public override string ToString()
    {
        return m_Name;
    }
}
