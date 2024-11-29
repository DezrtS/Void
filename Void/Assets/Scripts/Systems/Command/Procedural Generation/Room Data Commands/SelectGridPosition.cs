using UnityEngine;

public class SelectGridPosition : ICommand
{
    private readonly IHoldTilePositions tilePositionsHolder;
    private readonly Vector2Int position;

    public SelectGridPosition(IHoldTilePositions tilePositionsHolder, Vector2Int position)
    {
        this.tilePositionsHolder = tilePositionsHolder;
        this.position = position;
    }

    public void Execute()
    {
        tilePositionsHolder.TilePositions.Add(position);
    }

    public void Undo()
    {
        tilePositionsHolder.TilePositions.Remove(position);
    }
}
