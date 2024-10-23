using UnityEngine;

public class SelectGridPosition : ICommand
{
    private readonly RoomData roomData;
    private readonly Vector2Int position;

    public SelectGridPosition(RoomData roomData, Vector2Int position)
    {
        this.roomData = roomData;
        this.position = position;
    }

    public void Execute()
    {
        roomData.tilePositions.Add(position);
    }

    public void Undo()
    {
        roomData.tilePositions.Remove(position);
    }
}
