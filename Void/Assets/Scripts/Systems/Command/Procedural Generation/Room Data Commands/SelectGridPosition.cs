using UnityEngine;

public class SelectGridPosition : ICommand
{
    private readonly TileSection tileSection;
    private readonly Vector2Int position;

    public SelectGridPosition(TileSection tileSection, Vector2Int position)
    {
        this.tileSection = tileSection;
        this.position = position;
    }

    public void Execute()
    {
        tileSection.tilePositions.Add(position);
    }

    public void Undo()
    {
        tileSection.tilePositions.Remove(position);
    }
}
