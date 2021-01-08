using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Map
{
    public class Room
    {
        public static readonly Color RoomStarterColor = Color.magenta;

        public readonly ISet<Vector2Int> Cells = new HashSet<Vector2Int>();
        public readonly ISet<Vector2Int> EdgeCells = new HashSet<Vector2Int>();
        public readonly Vector2Int RoomStarterCell;
        public readonly Color Color;

        public Room(Vector2Int roomStarterCell, Color color)
        {
            RoomStarterCell = roomStarterCell;
            Color = color;
        }
    }

    public int Width { get; }
    public int Height { get; }
    public RectInt Rect => new RectInt(0, 0, Width, Height);
    private readonly MapCell[,] tiles;

    public Map(int w, int h)
    {
        Width = w;
        Height = h;
        tiles = new MapCell[Width, Height];
    }

    public bool IsWall(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return true;
        return (tiles[x, y] ?? MapCell.Empty).Type == MapCellType.Wall;
    }

    public void SetCellSafe(Vector2Int position, MapCell cell)
    {
        if (0 <= position.x && position.x < Width && 0 <= position.y && position.y < Height)
            SetCellInternal(position.x, position.y, cell);
    }

    public void SetCell(Vector2Int position, MapCell cell)
    {
        SetCell(position.x, position.y, cell);
    }

    public void SetCell(int x, int y, MapCell cell)
    {
        CheckCoords(x, y);
        SetCellInternal(x, y, cell);
    }

    private void SetCellInternal(int x, int y, MapCell cell)
    {
        if (tiles[x, y] is ColoredMapCell && cell is ColoredMapCell)
            return;
        tiles[x, y] = cell;
    }

    private void CheckCoords(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            throw new Exception($"Map coordinates ({x}, {y}) out of range.");
    }

    public void PutMap(int targetX, int targetY, Map map)
    {
        var mapArea = map.Rect;
        for (int y = 0; y < mapArea.height; y++)
        {
            for (int x = 0; x < mapArea.width; x++)
            {
                SetCell(targetX + x, targetY + y, MapCell.WallOrEmpty(map.IsWall(mapArea.x + x, mapArea.y + y)));
            }
        }
    }

    public int GetNeighborCount(int x, int y)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (IsWall(x + dx, y + dy))
                    ++count;
            }
        }

        return count;
    }

    public List<Room> FindRooms(RectInt area, FindRoomsOptions options = null)
    {
        options = options ?? FindRoomsOptions.Default;
        var result = new List<Room>();
        var processed = new HashSet<Vector2Int>();
        var usedColors = new HashSet<Color> {Room.RoomStarterColor};

        for (int y = 0; y < area.height; y++)
        {
            for (int x = 0; x < area.width; x++)
            {
                var cell = new Vector2Int(area.x + x, area.y + y);
                if (IsWall(x, y))
                    continue;

                if (!processed.Add(cell))
                    continue;

                var room = BuildRoom(CreateRoom(cell, usedColors), processed);
                UpdateEdgeCells(room);

                result.Add(room);
            }
        }

        return result;
    }

    private Room CreateRoom(Vector2Int starterCell, ISet<Color> usedColors)
    {
        return new Room(starterCell, PickRandomColor(usedColors));
    }

    private Color PickRandomColor(ISet<Color> usedColors)
    {
        Color result;
        var i = 0;
        do
        {
            if (usedColors.Add(result = Random.ColorHSV()))
                break;
        } while (++i < 5);

        return result;
    }

    private Room BuildRoom(Room room, ISet<Vector2Int> processed)
    {
        var stack = new Stack<Vector2Int>();
        stack.Push(room.RoomStarterCell);
        do
        {
            var nextCell = stack.Pop();
            if (IsWall(nextCell.x, nextCell.y))
                continue;
            processed.Add(nextCell);
            if (!room.Cells.Add(nextCell))
                continue;
            stack.Push(new Vector2Int(nextCell.x - 1, nextCell.y));
            stack.Push(new Vector2Int(nextCell.x + 1, nextCell.y));
            stack.Push(new Vector2Int(nextCell.x, nextCell.y - 1));
            stack.Push(new Vector2Int(nextCell.x, nextCell.y + 1));
        } while (stack.Count > 0);

        return room;
    }

    private static void UpdateEdgeCells(Room room)
    {
        foreach (var cell in room.Cells)
        {
            if (!room.Cells.Contains(new Vector2Int(cell.x - 1, cell.y)) ||
                !room.Cells.Contains(new Vector2Int(cell.x + 1, cell.y)) ||
                !room.Cells.Contains(new Vector2Int(cell.x, cell.y - 1)) ||
                !room.Cells.Contains(new Vector2Int(cell.x, cell.y + 1)))
            {
                room.EdgeCells.Add(cell);
            }
        }
    }

    public void Fill(RectInt area, MapCell cell)
    {
        for (int y = 0; y < area.height; y++)
        {
            for (int x = 0; x < area.width; x++)
                SetCell(area.x + x, area.y + y, cell);
        }
    }

    public void FillRandom(RectInt area, float threshold)
    {
        for (int y = 0; y < area.height; y++)
        {
            for (int x = 0; x < area.width; x++)
            {
                SetCell(area.x + x, area.y + y, MapCell.WallOrEmpty(Random.Range(0.0f, 1.0f) < threshold));
            }
        }
    }

    public void ApplyToTilemap(Tilemap tilemap, TileData tileData, Vector2Int target, RectInt area)
    {
        foreach (var position in area.allPositionsWithin)
        {
            tilemap.SetTile(new Vector3Int(target.x + position.x, target.y + position.y, 0), null);
        }

        foreach (var position in area.allPositionsWithin)
        {
            int x = position.x, y = position.y;
            var tilemapPosition = new Vector3Int(target.x + x, target.y + y, 0);
            var cell = tiles[area.x + x, area.y + y];
            switch (cell)
            {
                case EmptyMapCell _:
                    tilemap.SetTile(tilemapPosition, null);
                    break;
                case WallMapCell _:
                    tilemap.SetTile(tilemapPosition, tileData.wallTile);
                    break;
                case ColoredMapCell coloredCell:
                    tilemap.SetTile(tilemapPosition, tileData.wallTile);
                    tilemap.SetTileFlags(tilemapPosition, TileFlags.None);
                    tilemap.SetColor(tilemapPosition, coloredCell.Color);
                    break;
            }
        }
    }
}