using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoldTilePositions
{
    public Vector2Int GridSize { get; set; }
    [HideInInspector] public Vector2Int gridSize { get; set; }
    public List<Vector2Int> tilePositions { get; set; }
}
