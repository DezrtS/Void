using System.Collections.Generic;
using UnityEngine;

public class ResetGridPositions : ICommand
{
    private readonly RoomData roomData;
    private readonly List<Vector2Int> positions;

    public ResetGridPositions(RoomData roomData)
    {
        this.roomData = roomData;
        positions = new List<Vector2Int>(roomData.tilePositions);
    }

    public void Execute()
    {
        roomData.tilePositions.Clear();
    }

    public void Undo()
    {
        roomData.tilePositions = new List<Vector2Int>(positions);
    }
}