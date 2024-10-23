using UnityEngine;

public class DeselectGridPosition : ICommand
{
    private readonly RoomData roomData;
    private readonly Vector2Int position;

    public DeselectGridPosition(RoomData roomData, Vector2Int position)
    {
        this.roomData = roomData;
        this.position = position;
    }

    public void Execute()
    {
        roomData.tilePositions.Remove(position);
    }

    public void Undo()
    {
        roomData.tilePositions.Add(position);
    }
}