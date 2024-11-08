using UnityEngine;

public class DeselectGridPosition : ICommand
{
    private readonly TileSection tileSection;
    private readonly Vector2Int position;

    public DeselectGridPosition(TileSection tileSection, Vector2Int position)
    {
        this.tileSection = tileSection;
        this.position = position;
    }

    public void Execute()
    {
        tileSection.tilePositions.Remove(position);
    }

    public void Undo()
    {
        tileSection.tilePositions.Add(position);
    }
}