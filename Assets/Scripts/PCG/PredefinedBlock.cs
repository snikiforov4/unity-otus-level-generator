using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PredefinedBlock : MonoBehaviour
{
    private static readonly Vector2 LeftBottom = new Vector2(3f, 1.5f);
    private static readonly Vector2 RightBottom = new Vector2(9f, 1.5f);
    private static readonly Vector2 LeftUp = new Vector2(3f, 6f);
    private static readonly Vector2 RightUp = new Vector2(9f, 6f);
    private static readonly Vector2 PlayerStartPosition = LeftBottom;

    public static readonly Dictionary<int, Vector2> SpawnPoints = new Dictionary<int, Vector2>
    {
        [0] = LeftBottom,
        [1] = RightBottom,
        [2] = LeftUp,
        [3] = RightUp,
    };

    private const int X = -5;
    private const int Y = -3;

    public bool left;
    public bool right;
    public bool up;
    public bool down;

    private readonly Vector2Int[] m_LeftDoorCoords = {new Vector2Int(0, 3), new Vector2Int(0, 4)};
    private readonly Vector2Int[] m_RightDoorCoords = {new Vector2Int(Width - 1, 3), new Vector2Int(Width - 1, 4)};
    private readonly Vector2Int[] m_DownDoorCoords = {new Vector2Int(5, 0), new Vector2Int(6, 0)};
    private readonly Vector2Int[] m_UpDoorCoords = {new Vector2Int(5, Height - 1), new Vector2Int(6, Height - 1)};

    private bool _bitmaskInitialized;
    private int _bitmask;
    private Map _map;

    public const int Width = 12;
    public const int Height = 8;

    public int Bitmask
    {
        get
        {
            if (!_bitmaskInitialized)
                InitBitmask();

            return _bitmask;
        }
    }

    private void InitBitmask()
    {
        int result = 0;
        if (left)
            result |= Direction.Left.Bitmask;
        if (right)
            result |= Direction.Right.Bitmask;
        if (up)
            result |= Direction.Up.Bitmask;
        if (down)
            result |= Direction.Down.Bitmask;
        _bitmask = result;
        _bitmaskInitialized = true;
    }

    public Map Map
    {
        get
        {
            if (_map == null)
                InitMap();

            return _map;
        }
    }

    private void InitMap()
    {
        _map = new Map(Width, Height);
        var tilemap = GetComponentInChildren<Tilemap>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var coord = new Vector3Int(X + x, Y + y, 0);
                _map.SetCell(x, y, MapCell.WallOrEmpty(tilemap.GetTile(coord) != null));
            }
        }
    }

    public bool HasDoor(Direction direction)
    {
        return (Bitmask & direction.Bitmask) > 0;
    }

    public Vector2Int[] GetDoorwayCoords(Direction direction)
    {
        if (!HasDoor(direction))
            throw new Exception($"No door for direction={direction}");

        if (direction == Direction.Left)
            return m_LeftDoorCoords;
        if (direction == Direction.Right)
            return m_RightDoorCoords;
        if (direction == Direction.Up)
            return m_UpDoorCoords;
        if (direction == Direction.Down)
            return m_DownDoorCoords;

        throw new Exception($"No door coords for direction={direction}");
    }

    void Awake()
    {
        gameObject.SetActive(false);
    }


    public Vector2 GetPlayerPosition()
    {
        return PlayerStartPosition;
    }

    public override string ToString()
    {
        return $"{(down ? "D" : "-")} {(up ? "U" : "-")} {(right ? "R" : "-")} {(left ? "L" : "-")}";
    }
}