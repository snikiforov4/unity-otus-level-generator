using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Block
{
    private readonly bool[] occupiedSpawnPoint;
    
    public Block(PredefinedBlock predefinedBlock)
    {
        PredefinedBlock = predefinedBlock;
        occupiedSpawnPoint = Enumerable.Repeat(false, 4).ToArray();
    }

    public PredefinedBlock PredefinedBlock { get; }

    public int Bitmask => PredefinedBlock.Bitmask;

    public bool IsSpawnPointOccupied(int pointIdx)
    {
        if (pointIdx < 0 || pointIdx >= occupiedSpawnPoint.Length)
            return true;
        return occupiedSpawnPoint[pointIdx];
    }
    
    public void SetSpawnPoint(int point, bool occupied)
    {
        if (point < 0 || point >= occupiedSpawnPoint.Length)
            return;
        occupiedSpawnPoint[point] = occupied;
    }
    
    public bool HasFreeSpawnPoint()
    {
        return GetRandomSpawnPoint() >= 0;
    }
    
    public bool TryGetFreeSpawnPoint(out Vector2 point, out int pointRandomIdx)
    {
        point = Vector2.zero;
        pointRandomIdx = GetRandomSpawnPoint();
        if (pointRandomIdx >= 0)
        {
            point = PredefinedBlock.SpawnPoints[pointRandomIdx];
            return true;
        }

        return false;
    }
    
    private int GetRandomSpawnPoint()
    {
        var randomIdx = Random.Range(0, occupiedSpawnPoint.Length);
        for (int i = 0; i < occupiedSpawnPoint.Length; i++)
        {
            var idx = (randomIdx + i) % occupiedSpawnPoint.Length;
            if (!occupiedSpawnPoint[idx])
                return idx;
        }

        return -1;
    }
}