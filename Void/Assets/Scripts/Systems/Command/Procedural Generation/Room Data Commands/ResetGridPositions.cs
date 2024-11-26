using System.Collections.Generic;
using UnityEngine;

public class ResetGridPositions : ICommand
{
    private readonly IHoldTilePositions tilePositionsHolder;
    private readonly List<Vector2Int> positions;

    public ResetGridPositions(IHoldTilePositions tilePositionsHolder)
    {
        this.tilePositionsHolder = tilePositionsHolder;
        positions = new List<Vector2Int>(tilePositionsHolder.tilePositions);
    }

    public void Execute()
    {
        tilePositionsHolder.tilePositions.Clear();
    }

    public void Undo()
    {
        tilePositionsHolder.tilePositions = new List<Vector2Int>(positions);
    }
}