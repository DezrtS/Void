using UnityEngine;

public class DeselectGridPosition : ICommand
{
    private readonly IHoldTilePositions tilePositionsHolder;
    private readonly Vector2Int position;

    public DeselectGridPosition(IHoldTilePositions tilePositionsHolder, Vector2Int position)
    {
        this.tilePositionsHolder = tilePositionsHolder;
        this.position = position;
    }

    public void Execute()
    {
        tilePositionsHolder.tilePositions.Remove(position);
    }

    public void Undo()
    {
        tilePositionsHolder.tilePositions.Add(position);
    }
}