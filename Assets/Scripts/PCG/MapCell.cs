using UnityEngine;

public abstract class MapCell
{
    public static readonly MapCell Empty = new EmptyMapCell();
    public static readonly MapCell Wall = new WallMapCell();

    public readonly MapCellType Type;

    protected MapCell(MapCellType type)
    {
        Type = type;
    }

    public static MapCell WallOrEmpty(bool wall)
    {
        return wall ? Wall : Empty;
    }
}

internal class EmptyMapCell : MapCell
{
    public EmptyMapCell() : base(MapCellType.Empty)
    {
    }
}

internal class WallMapCell : MapCell
{
    public WallMapCell(Color? color = null) : base(MapCellType.Wall)
    {
    }
}

public class ColoredMapCell : MapCell
{
    public Color Color { get; }

    public ColoredMapCell(Color color) : base(MapCellType.Empty)
    {
        Color = color;
    }
}