using System.Collections.Generic;
using UnityEngine;

public class PredefinedBlockGenerator : AbstractGenerator
{
    int widthInBlocks;
    int heightInBlocks;
    private PredefinedBlock[] predefinedBlocks;
    Dictionary<Vector2Int, int> blocksBitmasks;

    public Transform character;

    protected override void GenerateLevel()
    {
        InitializeData();
        int startX = Random.Range(0, widthInBlocks);
        int startY = Random.Range(0, heightInBlocks);
        var startBlock = GetRandomBlockMatchingMask(0, GetRequireAbsentMask(new Vector2Int(startX, startY)));
        PutBlock(startX, startY, startBlock);
        SetCharacterPosition(startX, startY, startBlock.GetPlayerPosition());

        blocksBitmasks = new Dictionary<Vector2Int, int>();
        blocksBitmasks[new Vector2Int(startX, startY)] = startBlock.Bitmask;

        var stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        do
        {
            Vector2Int pos = stack.Pop();
            int bitmask = blocksBitmasks[pos];

            if ((bitmask & Direction.Left.Bitmask) != 0 &&
                !blocksBitmasks.ContainsKey(new Vector2Int(pos.x - 1, pos.y)))
            {
                stack.Push(new Vector2Int(pos.x - 1, pos.y));
                SpawnNode(new Vector2Int(pos.x - 1, pos.y));
            }

            if ((bitmask & Direction.Right.Bitmask) != 0 &&
                !blocksBitmasks.ContainsKey(new Vector2Int(pos.x + 1, pos.y)))
            {
                stack.Push(new Vector2Int(pos.x + 1, pos.y));
                SpawnNode(new Vector2Int(pos.x + 1, pos.y));
            }

            if ((bitmask & Direction.Down.Bitmask) != 0 &&
                !blocksBitmasks.ContainsKey(new Vector2Int(pos.x, pos.y - 1)))
            {
                stack.Push(new Vector2Int(pos.x, pos.y - 1));
                SpawnNode(new Vector2Int(pos.x, pos.y - 1));
            }

            if ((bitmask & Direction.Up.Bitmask) != 0 &&
                !blocksBitmasks.ContainsKey(new Vector2Int(pos.x, pos.y + 1)))
            {
                stack.Push(new Vector2Int(pos.x, pos.y + 1));
                SpawnNode(new Vector2Int(pos.x, pos.y + 1));
            }
        } while (stack.Count > 0);
    }

    private void InitializeData()
    {
        map.Fill(map.Rect, MapCell.Empty);
        predefinedBlocks = FindObjectsOfType<PredefinedBlock>();
        Debug.Log($"Found predefined blocks count={predefinedBlocks.Length}");
        widthInBlocks = width / PredefinedBlock.Width;
        heightInBlocks = height / PredefinedBlock.Height;
    }

    private int GetRequireAbsentMask(Vector2Int pos)
    {
        int requireAbsent = 0;

        if (pos.x == 0)
            requireAbsent |= Direction.Left.Bitmask;
        else if (pos.x == widthInBlocks - 1)
            requireAbsent |= Direction.Right.Bitmask;

        if (pos.y == 0)
            requireAbsent |= Direction.Down.Bitmask;
        else if (pos.y == heightInBlocks - 1)
            requireAbsent |= Direction.Up.Bitmask;

        return requireAbsent;
    }

    private PredefinedBlock GetRandomBlockMatchingMask(int requirePresent, int requireAbsent)
    {
        var list = GetBlocksMatchingMask(requirePresent, requireAbsent);
        return list[Random.Range(0, list.Count)];
    }

    private List<PredefinedBlock> GetBlocksMatchingMask(int requirePresent, int requireAbsent)
    {
        var result = new List<PredefinedBlock>();
        foreach (var block in predefinedBlocks)
        {
            int bitmask = block.Bitmask;
            if ((bitmask & requirePresent) == requirePresent && (bitmask & requireAbsent) == 0)
                result.Add(block);
        }

        return result;
    }

    private void PutBlock(int blockX, int blockY, PredefinedBlock block)
    {
        var blockMap = block.Map;
        Debug.Log($"({blockX}, {blockY}) block={block}");
        map.PutMap(
            blockX * PredefinedBlock.Width,
            blockY * PredefinedBlock.Height,
            blockMap
        );
    }

    private void SetCharacterPosition(int blockX, int blockY, Vector3 posInBlock)
    {
        int tileX = blockX * PredefinedBlock.Width;
        int tileY = blockY * PredefinedBlock.Height;
        var tilePos = tilemap.layoutGrid.CellToWorld(new Vector3Int(tileX, tileY, 0));
        character.position = posInBlock + tilePos;
    }
    
    private void SpawnNode(Vector2Int pos)
    {
        int requirePresent = 0;
        int requireAbsent = GetRequireAbsentMask(pos);
        int bitmask;

        List<int> empty = new List<int>();

        if (blocksBitmasks.TryGetValue(new Vector2Int(pos.x - 1, pos.y), out bitmask))
        {
            if ((bitmask & Direction.Right.Bitmask) != 0)
                requirePresent |= Direction.Left.Bitmask;
            else
                requireAbsent |= Direction.Left.Bitmask;
        }
        else if ((requireAbsent & Direction.Left.Bitmask) == 0)
            empty.Add(Direction.Left.Bitmask);

        if (blocksBitmasks.TryGetValue(new Vector2Int(pos.x + 1, pos.y), out bitmask))
        {
            if ((bitmask & Direction.Left.Bitmask) != 0)
                requirePresent |= Direction.Right.Bitmask;
            else
                requireAbsent |= Direction.Right.Bitmask;
        }
        else if ((requireAbsent & Direction.Right.Bitmask) == 0)
            empty.Add(Direction.Right.Bitmask);

        if (blocksBitmasks.TryGetValue(new Vector2Int(pos.x, pos.y - 1), out bitmask))
        {
            if ((bitmask & Direction.Up.Bitmask) != 0)
                requirePresent |= Direction.Down.Bitmask;
            else
                requireAbsent |= Direction.Down.Bitmask;
        }
        else if ((requireAbsent & Direction.Down.Bitmask) == 0)
            empty.Add(Direction.Down.Bitmask);

        if (blocksBitmasks.TryGetValue(new Vector2Int(pos.x, pos.y + 1), out bitmask))
        {
            if ((bitmask & Direction.Down.Bitmask) != 0)
                requirePresent |= Direction.Up.Bitmask;
            else
                requireAbsent |= Direction.Up.Bitmask;
        }
        else if ((requireAbsent & Direction.Up.Bitmask) == 0)
            empty.Add(Direction.Up.Bitmask);

        if (empty.Count > 0)
            requirePresent |= empty[Random.Range(0, empty.Count)];

        PredefinedBlock block = GetRandomBlockMatchingMask(requirePresent, requireAbsent);
        PutBlock(pos.x, pos.y, block);

        blocksBitmasks[pos] = block.Bitmask;
    }
}