using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoldTilePositions
{
    public Vector2Int GridSize { get; set; }
    public List<Vector2Int> TilePositions { get; set; }
}