using System.Collections.Generic;
using UnityEngine;

public class CellularAutomataGenerator : AbstractGenerator
{
    [Range(0, 100)]
    public int threshold;

    public int minNeighbors;
    public int smoothSteps;

    [ContextMenu("Smooth")]
    public void Smooth()
    {
        Smooth(map, map.Rect, minNeighbors);
        Apply();
    }

    protected override void GenerateLevel()
    {
        map.FillRandom(map.Rect, threshold / 100.0f);

        for (int i = 0; i < smoothSteps; i++)
            Smooth(map, map.Rect, minNeighbors);

        ConnectRooms(map);
    }

    private static void Smooth(Map map, RectInt area, int min)
    {
        for (int y = 0; y < area.height; y++) {
            for (int x = 0; x < area.width; x++) {
                int count = map.GetNeighborCount(area.x + x, area.y + y);
                map.SetCell(area.x + x, area.y + y, MapCell.WallOrEmpty(count > min));
            }
        }
    }

    private static void ConnectRooms(Map map)
    {
        var connectedRooms = new HashSet<(Map.Room, Map.Room)>();

        List<Map.Room> rooms = map.FindRooms(map.Rect);
        foreach (Map.Room room1 in rooms) {
            Map.Room closestRoom = null;
            float closestDistance = 0.0f;
            Vector2Int closestCell1 = Vector2Int.zero;
            Vector2Int closestCell2 = Vector2Int.zero;

            foreach (Map.Room room2 in rooms) {
                if (room1 == room2)
                    continue;

                if (connectedRooms.Contains((room1, room2))) {
                    closestRoom = null;
                    break;
                }

                foreach (var cell1 in room1.EdgeCells) {
                    foreach (var cell2 in room2.EdgeCells) {
                        float distance = Vector2Int.Distance(cell1, cell2);
                        if (closestRoom == null || distance < closestDistance) {
                            closestDistance = distance;
                            closestRoom = room2;
                            closestCell1 = cell1;
                            closestCell2 = cell2;
                        }
                    }
                }
            }

            if (closestRoom != null) {
                connectedRooms.Add((room1, closestRoom));
                connectedRooms.Add((closestRoom, room1));
                CreatePassage(map, closestCell1, closestCell2);
            }
        }
    }

    private static void CreatePassage(Map map, Vector2Int start, Vector2Int end)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int d = 2 * dy - dx;

        int y = start.y;
        for (int x = start.x; x <= end.x; x++) {
            map.Fill(new RectInt(x - 1, y - 1, 3, 3), MapCell.Empty);
            if (d > 0) {
                ++y;
                d = d - 2 * dx;
            }
            d = d + 2 * dy;
        }
    }
}
