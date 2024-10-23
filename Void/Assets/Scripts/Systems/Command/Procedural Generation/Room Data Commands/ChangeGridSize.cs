using System.Collections.Generic;
using UnityEngine;

public class ChangeGridSize : ICommand
{
    private readonly RoomData roomData;
    private readonly List<Vector2Int> removedPositions;
    private readonly Vector2Int previousSize;
    private readonly Vector2Int size;

    public ChangeGridSize(RoomData roomData, Vector2Int size)
    {
        this.roomData = roomData;
        removedPositions = new List<Vector2Int>();
        foreach (Vector2Int position in roomData.tilePositions)
        {
            if (position.x >= size.x || position.y >= size.y)
            {
                removedPositions.Add(position);
            }
        }

        previousSize = roomData.gridSize;
        this.size = size;
    }

    public void Execute()
    {
        roomData.gridSize = size;
        foreach (var position in removedPositions)
        {
            roomData.tilePositions.Remove(position);
        }
    }

    public void Undo()
    {
        roomData.gridSize = previousSize;
        roomData.tilePositions.AddRange(removedPositions);
    }
}