using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MixedGenerator : AbstractGenerator
{
    private int widthInBlocks;
    private int heightInBlocks;
    private PredefinedBlock[] predefinedBlocks;
    private Block[,] blocks;
    private GameObject[] coins;

    public PredefinedBlocksData predefinedBlockData;
    public CellularAutomataData cellularAutomataData;
    public SpawnedObjectsData spawnedObjectsData;

    [Serializable]
    public struct PredefinedBlocksData
    {
        [Range(0, 100)] public float rate;
    }

    [Serializable]
    public struct CellularAutomataData
    {
        [Range(0, 100)] public float randomRate;
        public int minNeighbors;
        public int smoothSteps;
    }

    [Serializable]
    public struct SpawnedObjectsData
    {
        public Transform character;
        public GameObject coinPrefab;
        public int coinsCount;
        public GameObject coinsGroup;
    }

    protected override void GenerateLevel()
    {
        PrepareData();
        GeneratePredefinedBlocks();
        var freeBlocks = blocks.GetAllNonOccupiedBlocks();
        FillRandomFreeBlocks(freeBlocks);
        SmoothBlocks(freeBlocks);
        PutLevelBorders();
        for (int i = 0; i < 5; i++)
            ConnectRooms();
        RemoveBlockDoorWithoutPassage();

        UpdateCharacterPosition();
    }

    private void PrepareData()
    {
        map.Fill(map.Rect, MapCell.Empty);
        predefinedBlocks = FindObjectsOfType<PredefinedBlock>();
        Debug.Log($"predefined blocks found: {predefinedBlocks.Length} ");
        widthInBlocks = width / PredefinedBlock.Width;
        heightInBlocks = height / PredefinedBlock.Height;
        blocks = new Block[widthInBlocks, heightInBlocks];
        ClearSpawnedCoins();
    }
    
    [ContextMenu("Clear Spawned Coins")]
    private void ClearSpawnedCoins()
    {
        if (spawnedObjectsData.coinsGroup == null) return;
        foreach (var coin in spawnedObjectsData.coinsGroup.GetComponentsInChildren<Coin>())
        {
            GameObjectsUtils.SafeDestroyGameObject(coin);
        }
    }

    private void GeneratePredefinedBlocks()
    {
        int totalNumberOfBlocks = GetTotalNumberOfBlocks();
        int blockToGenerated = (int) (totalNumberOfBlocks / 100.0f * predefinedBlockData.rate);
        Debug.Log($"total={totalNumberOfBlocks} generate={blockToGenerated}");
        for (int i = 0; i < blockToGenerated; i++)
        {
            if (RandomlyPickBlock(b => b == null, out var blockPosition))
            {
                SpawnNode(blockPosition);
                continue;
            }

            break;
        }
    }

    private int GetTotalNumberOfBlocks()
    {
        return widthInBlocks * heightInBlocks;
    }

    private bool RandomlyPickBlock(Predicate<Block> match, out Vector2Int position)
    {
        int x = Random.Range(0, widthInBlocks);
        int y = Random.Range(0, heightInBlocks);
        if (match(blocks[x, y]))
        {
            position = new Vector2Int(x, y);
            return true;
        }

        int maxAttempts = GetTotalNumberOfBlocks() - 1;
        int attempts = 0;
        do
        {
            x++;
            if (x >= widthInBlocks)
            {
                x = 0;
                y++;
            }

            if (y >= heightInBlocks)
            {
                y = 0;
            }

            if (match(blocks[x, y]))
            {
                position = new Vector2Int(x, y);
                return true;
            }
        } while (++attempts < maxAttempts);

        position = Vector2Int.zero;
        return false;
    }

    private void SpawnNode(Vector2Int pos)
    {
        int requirePresent = 0;
        int requireAbsent = GetRequireAbsentMask(pos);

        List<int> empty = new List<int>();

        if (blocks.TryGetBitmask(pos.x - 1, pos.y, out int bitmask))
        {
            if ((bitmask & Direction.Right.Bitmask) != 0)
                requirePresent |= Direction.Left.Bitmask;
            else
                requireAbsent |= Direction.Left.Bitmask;
        }
        else if ((requireAbsent & Direction.Left.Bitmask) == 0)
            empty.Add(Direction.Left.Bitmask);

        if (blocks.TryGetBitmask(pos.x + 1, pos.y, out bitmask))
        {
            if ((bitmask & Direction.Left.Bitmask) != 0)
                requirePresent |= Direction.Right.Bitmask;
            else
                requireAbsent |= Direction.Right.Bitmask;
        }
        else if ((requireAbsent & Direction.Right.Bitmask) == 0)
            empty.Add(Direction.Right.Bitmask);

        if (blocks.TryGetBitmask(pos.x, pos.y - 1, out bitmask))
        {
            if ((bitmask & Direction.Up.Bitmask) != 0)
                requirePresent |= Direction.Down.Bitmask;
            else
                requireAbsent |= Direction.Down.Bitmask;
        }
        else if ((requireAbsent & Direction.Down.Bitmask) == 0)
            empty.Add(Direction.Down.Bitmask);

        if (blocks.TryGetBitmask(pos.x, pos.y + 1, out bitmask))
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

        var block = GetRandomBlockMatchingMask(requirePresent, requireAbsent);
        PutBlock(pos.x, pos.y, block);
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

    private bool TryGetBlockMatchingMask(int mask, out PredefinedBlock predefinedBlock)
    {
        foreach (var block in predefinedBlocks)
        {
            if (block.Bitmask != mask) continue;
            predefinedBlock = block;
            return true;
        }

        predefinedBlock = null;
        return false;
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
        blocks[blockX, blockY] = new Block(block);
    }

    private void FillRandomFreeBlocks(IList<Vector2Int> freeBlocks)
    {
        var threshold = cellularAutomataData.randomRate / 100.0f;
        foreach (var block in freeBlocks)
        {
            map.FillRandom(CalcRectForBlock(block), threshold);
        }
    }

    private static RectInt CalcRectForBlock(Vector2Int block)
    {
        return new RectInt(block.x * PredefinedBlock.Width, block.y * PredefinedBlock.Height,
            PredefinedBlock.Width, PredefinedBlock.Height);
    }

    private void SmoothBlocks(IList<Vector2Int> freeBlocks)
    {
        for (int i = 0; i < cellularAutomataData.smoothSteps; i++)
        {
            foreach (var block in freeBlocks)
                Smooth(map, CalcRectForBlock(block), cellularAutomataData.minNeighbors);
        }
    }

    private static void Smooth(Map map, RectInt area, int min)
    {
        foreach (var position in area.allPositionsWithin)
        {
            int x = position.x, y = position.y;
            int count = map.GetNeighborCount(x, y);
            map.SetCell(x, y, MapCell.WallOrEmpty(count > min));
        }
    }

    private void ConnectRooms()
    {
        var rooms = map.FindRooms(map.Rect);
        Debug.Log($"Connect Rooms: number of rooms={rooms.Count}");

        var connectedRooms = new HashSet<(Map.Room, Map.Room)>();
        foreach (var room1 in rooms)
        {
            Map.Room closestRoom = null;
            float closestDistance = float.MaxValue;
            Vector2Int closestCell1 = Vector2Int.zero;
            Vector2Int closestCell2 = Vector2Int.zero;

            foreach (var room2 in rooms)
            {
                if (room1 == room2)
                    continue;

                if (connectedRooms.Contains((room1, room2)))
                {
                    Debug.Log("Connect Rooms: Connection already exists");
                    closestRoom = null;
                    break;
                }

                foreach (var cell1 in room1.EdgeCells)
                {
                    foreach (var cell2 in room2.EdgeCells)
                    {
                        float distance = Vector2Int.Distance(cell1, cell2);
                        if (closestRoom == null || distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestRoom = room2;
                            closestCell1 = cell1;
                            closestCell2 = cell2;
                        }
                    }
                }
            }

            if (closestRoom != null)
            {
                connectedRooms.Add((room1, closestRoom));
                connectedRooms.Add((closestRoom, room1));

                closestCell1 = UpdateCellIfPredefinedBlock(closestCell1, closestCell2);
                closestCell2 = UpdateCellIfPredefinedBlock(closestCell2, closestCell1);

                CreatePassage(map, closestCell1, closestCell2);
            }
            else
            {
                Debug.Log($"Connect Rooms: Closest room not found!");
            }
        }
    }

    private Vector2Int UpdateCellIfPredefinedBlock(Vector2Int from, Vector2Int to)
    {
        var result = from;
        int dx = from.x - to.x;
        int dy = from.y - to.y;
        int x = from.x / PredefinedBlock.Width;
        int y = from.y / PredefinedBlock.Height;
        if (blocks.Contains(x, y))
        {
            Debug.Log($"Predefined block ({x}, {y})");
            Direction dir;
            if (Math.Abs(dx) > Math.Abs(dy))
                dir = dx > 0 ? Direction.Left : Direction.Right;
            else
                dir = dy > 0 ? Direction.Down : Direction.Up;

            var predefinedBlock = blocks[x, y].PredefinedBlock;
            int newBitmask = predefinedBlock.Bitmask | dir.Bitmask;
            if (TryGetBlockMatchingMask(newBitmask, out var newPredefinedBlock))
            {
                PutBlock(x, y, newPredefinedBlock);
                var doorwayCoords = newPredefinedBlock.GetDoorwayCoords(dir);
                var randomDoorwayCell = doorwayCoords[Random.Range(0, doorwayCoords.Length)];
                result = new Vector2Int(x * PredefinedBlock.Width + randomDoorwayCell.x,
                    y * PredefinedBlock.Height + randomDoorwayCell.y);
            }
        }

        return result;
    }

    private static void CreatePassage(Map map, Vector2Int start, Vector2Int end)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int steps = Mathf.RoundToInt(Vector2Int.Distance(start, end));

        float stepX = (float) dx / steps;
        float stepY = (float) dy / steps;
        Debug.Log($"start={start} end={end} dx={dx} dy={dy} stepX={stepX} stepY={stepY} distance={steps}");

        float x = start.x, y = start.y;
        for (int i = 0; i < steps; x += stepX, y += stepY, i++)
        {
            var position = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
            // var cell = new ColoredMapCell(Color.cyan);
            var cell = MapCell.Empty;
            map.SetCell(position, cell);
            map.SetCellSafe(position + Vector2Int.left, cell);
            map.SetCellSafe(position + Vector2Int.right, cell);
            map.SetCellSafe(position + Vector2Int.up, cell);
            map.SetCellSafe(position + Vector2Int.down, cell);
        }
    }

    private void RemoveBlockDoorWithoutPassage()
    {
        foreach (var blockPositions in blocks.GetAllOccupiedBlocks())
        {
            int x = blockPositions.x, y = blockPositions.y;
            var block = blocks[x, y];
            var passageChecker = new BlockPassageChecker(x, y, block.PredefinedBlock, map);
            var currentBitmask = block.Bitmask;
            var newBitmask = currentBitmask;
            foreach (var direction in Direction.All)
            {
                if (block.PredefinedBlock.HasDoor(direction) && !passageChecker.HasPassage(direction))
                {
                    newBitmask &= ~direction.Bitmask;
                    Debug.Log($"block=({x}, {y}) need to remove {direction} door");
                }
            }

            if (currentBitmask != newBitmask)
            {
                if (TryGetBlockMatchingMask(newBitmask, out var newPredefinedBlock))
                {
                    PutBlock(x, y, newPredefinedBlock);
                }
                else
                {
                    Debug.Log($"block=({x}, {y}) cannot find block by mask={newBitmask}");
                }
            }
        }
    }

    private void UpdateCharacterPosition()
    {
        if (RandomlyPickBlock(b => b != null, out var blockPosition))
        {
            var block = blocks[blockPosition.x, blockPosition.y];
            var charPos = block.PredefinedBlock.GetPlayerPosition();
            int tileX = blockPosition.x * PredefinedBlock.Width;
            int tileY = blockPosition.y * PredefinedBlock.Height;
            var tilePos = tilemap.layoutGrid.CellToWorld(new Vector3Int(tileX, tileY, 0));
            spawnedObjectsData.character.position = tilePos + new Vector3(charPos.x, charPos.y);
        }
    }

    [ContextMenu("Spawn Coins")]
    private void SpawnCoins()
    {
        for (int i = 0; i < spawnedObjectsData.coinsCount; i++)
        {
            if (RandomlyPickBlock(b => b != null && b.HasFreeSpawnPoint(), out var blockPosition))
            {
                Debug.Log("Picked random block");
                var block = blocks[blockPosition.x, blockPosition.y];
                if (block.TryGetFreeSpawnPoint(out var coinRelativePos, out var pointIdx))
                {
                    int tileX = blockPosition.x * PredefinedBlock.Width;
                    int tileY = blockPosition.y * PredefinedBlock.Height;
                    var tilePos = tilemap.layoutGrid.CellToWorld(new Vector3Int(tileX, tileY, 0));
                    var coinWorldPosition = tilePos + new Vector3(coinRelativePos.x, coinRelativePos.y);
                    Instantiate(spawnedObjectsData.coinPrefab, coinWorldPosition, Quaternion.identity,
                        spawnedObjectsData.coinsGroup.transform);
                    block.SetSpawnPoint(pointIdx, true);
                }
            }
        }
    }

    private void PutLevelBorders()
    {
        for (int i = 0; i < width; i++)
            map.SetCell(i, 0, MapCell.Wall);

        for (int i = 0; i < height; i++)
            map.SetCell(0, i, MapCell.Wall);

        for (int i = 0; i < width; i++)
            map.SetCell(i, height - 1, MapCell.Wall);

        for (int i = 0; i < height; i++)
            map.SetCell(width - 1, i, MapCell.Wall);
    }

    [ContextMenu("Paint Rooms")]
    public void PaintRooms()
    {
        PaintExistingRooms();
        Apply();
    }

    private void PaintExistingRooms()
    {
        var rooms = map.FindRooms(map.Rect, new FindRoomsOptions(true));
        Debug.Log($"Found rooms={rooms.Count}");
        foreach (var room in rooms)
        {
            map.SetCell(room.RoomStarterCell.x, room.RoomStarterCell.y,
                new ColoredMapCell(Map.Room.RoomStarterColor));
            foreach (var cell in room.Cells)
            {
                map.SetCell(cell.x, cell.y, new ColoredMapCell(room.Color));
            }
        }
    }

    [ContextMenu("Find Stuck Rooms")]
    private void FindPredefinedBlocksWithoutPassage()
    {
        foreach (var position in blocks.GetAllOccupiedBlocks())
        {
            int x = position.x, y = position.y;
            var block = blocks[x, y].PredefinedBlock;
            var passageChecker = new BlockPassageChecker(x, y, block, map);
            bool hasPassage = false;
            Debug.Log($"Find Passages for block=({x}, {y})");
            foreach (var direction in Direction.All)
            {
                if (block.HasDoor(direction) && passageChecker.HasPassage(direction))
                {
                    hasPassage = true;
                    break;
                }

                Debug.Log($"block=({x}, {y}) dir={direction} no passage");
            }

            if (!hasPassage)
            {
                Debug.Log($"block=({x}, {y}) ANY passage!");
            }
        }
    }
}