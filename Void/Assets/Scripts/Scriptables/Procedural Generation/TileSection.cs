using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileSection : ScriptableObject
{
    public Vector2Int GridSize;
    [HideInInspector] public Vector2Int gridSize = Vector2Int.one;
    public List<Vector2Int> tilePositions;
}