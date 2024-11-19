using System.Collections.Generic;
using UnityEngine;

public class ChangeGridSize : ICommand
{
    private readonly TileSection tileSection;
    private readonly List<Vector2Int> removedPositions;
    private readonly Vector2Int previousSize;
    private readonly Vector2Int size;

    public ChangeGridSize(TileSection tileSection, Vector2Int size)
    {
        this.tileSection = tileSection;
        removedPositions = new List<Vector2Int>();
        foreach (Vector2Int position in tileSection.tilePositions)
        {
            if (position.x >= size.x || position.y >= size.y)
            {
                removedPositions.Add(position);
            }
        }

        previousSize = tileSection.gridSize;
        this.size = size;
    }

    public void Execute()
    {
        tileSection.gridSize = size;
        foreach (var position in removedPositions)
        {
            tileSection.tilePositions.Remove(position);
        }
    }

    public void Undo()
    {
        tileSection.gridSize = previousSize;
        tileSection.tilePositions.AddRange(removedPositions);
    }
}