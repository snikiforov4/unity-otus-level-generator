using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class BlockPassageChecker
{
    private readonly PredefinedBlock block;
    private readonly Map map;
    private readonly Vector2Int globalCoords;

    public BlockPassageChecker(int x, int y, PredefinedBlock block, Map map)
    {
        this.block = block;
        this.map = map;
        this.globalCoords = CalcBlockGlobalCoords(x, y);
    }

    private static Vector2Int CalcBlockGlobalCoords(int x, int y)
    {
        return new Vector2Int(x * PredefinedBlock.Width, y * PredefinedBlock.Height);
    }

    public bool HasPassage(Direction direction)
    {
        return block.HasDoor(direction) && GetDoorNeighbours(direction).Any(IsNotWall);
    }

    private IList<Vector2Int> GetDoorNeighbours(Direction direction)
    {
        var offset = GetOffsetFor(direction);
        return block.GetDoorwayCoords(direction)
            .Select(cell => globalCoords + cell + offset)
            .ToList();
    }

    private Vector2Int GetOffsetFor(Direction direction)
    {
        if (direction == Direction.Left)
            return Vector2Int.left;
        if (direction == Direction.Right)
            return Vector2Int.right;
        if (direction == Direction.Up)
            return Vector2Int.up;
        if (direction == Direction.Down)
            return Vector2Int.down;
        throw new Exception($"Offset is not specified for direction={direction}");
    }

    private bool IsNotWall(Vector2Int position)
    {
        return !map.IsWall(position.x, position.y);
    }
}