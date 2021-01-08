using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockArray2D
{
    public static bool Contains(this Block[,] blocks, Vector2Int position)
    {
        return blocks.Contains(position.x, position.y);
    }

    public static bool Contains(this Block[,] blocks, int x, int y)
    {
        return blocks[x, y] != null;
    }

    public static bool TryGetBitmask(this Block[,] blocks, int x, int y, out int bitmask)
    {
        bitmask = 0;
        if (x < 0 || y < 0 || x >= blocks.GetLength(1) || y >= blocks.GetLength(0))
            return false;
        if (blocks[x, y] == null) 
            return false;
        bitmask = blocks[x, y].Bitmask;
        return true;
    }
    
    public static IList<Vector2Int> GetAllOccupiedBlocks(this Block[,] blocks)
    {
        return GetPositionMatches(blocks, block => block != null);
    }
    
    public static IList<Vector2Int> GetAllNonOccupiedBlocks(this Block[,] blocks)
    {
        return GetPositionMatches(blocks, block => block == null);
    }

    private static IList<Vector2Int> GetPositionMatches(Block[,] blocks, Predicate<Block> matcher)
    {
        var result = new List<Vector2Int>();
        for (int y = 0; y < blocks.GetLength(0); y++)
        {
            for (int x = 0; x < blocks.GetLength(1); x++)
            {
                if (matcher(blocks[x, y]))
                    result.Add(new Vector2Int(x, y));
            }
        }

        return result;
    }
}