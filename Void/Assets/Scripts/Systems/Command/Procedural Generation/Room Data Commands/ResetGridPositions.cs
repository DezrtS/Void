using System.Collections.Generic;
using UnityEngine;

public class ResetGridPositions : ICommand
{
    private readonly IHoldTilePositions tilePositionsHolder;
    private readonly List<Vector2Int> positions;

    public ResetGridPositions(IHoldTilePositions tilePositionsHolder)
    {
        this.tilePositionsHolder = tilePositionsHolder;
        positions = new List<Vector2Int>(tilePositionsHolder.TilePositions);
    }

    public void Execute()
    {
        tilePositionsHolder.TilePositions.Clear();
    }

    public void Undo()
    {
        tilePositionsHolder.TilePositions = new List<Vector2Int>(positions);
    }
}