using System.Collections.Generic;
using UnityEngine;

public class ChangeGridSize : ICommand
{
    private readonly IHoldTilePositions tilePositionsHolder;
    private readonly List<Vector2Int> removedPositions;
    private readonly Vector2Int previousSize;
    private readonly Vector2Int size;

    public ChangeGridSize(IHoldTilePositions tilePositionsHolder, Vector2Int size)
    {
        this.tilePositionsHolder = tilePositionsHolder;
        removedPositions = new List<Vector2Int>();
        foreach (Vector2Int position in tilePositionsHolder.TilePositions)
        {
            if (position.x >= size.x || position.y >= size.y)
            {
                removedPositions.Add(position);
            }
        }

        previousSize = tilePositionsHolder.Size;
        this.size = size;
    }

    public void Execute()
    {
        tilePositionsHolder.Size = size;
        foreach (var position in removedPositions)
        {
            tilePositionsHolder.TilePositions.Remove(position);
        }
    }

    public void Undo()
    {
        tilePositionsHolder.Size = previousSize;
        tilePositionsHolder.TilePositions.AddRange(removedPositions);
    }
}