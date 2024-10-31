using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Procedural Generation/RoomData", order = 1)]
public class RoomData : ScriptableObject
{
    [HideInInspector] public Vector2Int gridSize;
    public Vector2Int GridSize;
    [HideInInspector] public List<Vector2Int> tilePositions;
    public List<(Vector2Int from, Vector2Int to)> connections;
    public bool SpawnTiles;
    public bool HasInterior;
    public GameObject RoomPrefab;
}
