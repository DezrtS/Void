using System.Collections.Generic;
using UnityEngine;

public class ResetGridPositions : ICommand
{
    private readonly TileSection tileSection;
    private readonly List<Vector2Int> positions;

    public ResetGridPositions(TileSection tileSection)
    {
        this.tileSection = tileSection;
        positions = new List<Vector2Int>(tileSection.tilePositions);
    }

    public void Execute()
    {
        tileSection.tilePositions.Clear();
    }

    public void Undo()
    {
        tileSection.tilePositions = new List<Vector2Int>(positions);
    }
}