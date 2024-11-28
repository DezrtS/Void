using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Connection
{
    public Vector2Int from;
    public Vector2Int to;
}

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Procedural Generation/RoomData", order = 1)]
public class RoomData : ScriptableObject, IHoldTilePositions
{
    public Vector2Int ChangeGridSize;
    public Vector2Int GridSize { get { return ChangeGridSize; } set { ChangeGridSize = value; } }
    [HideInInspector] public Vector2Int gridSize { get; set; } 
    public List<Vector2Int> tilePositions { get; set; }
    public List<Connection> connections = new List<Connection>();
    public bool SpawnTiles;
    public bool HasInterior;
    public GameObject RoomPrefab;
}
