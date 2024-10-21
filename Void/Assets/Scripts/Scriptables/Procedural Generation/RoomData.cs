using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Procedural Generation/RoomData", order = 1)]
public class RoomData : ScriptableObject
{
    public Vector2Int gridSize;
    [HideInInspector] public List<Vector2Int> tilePositions;
    public List<(Vector2Int from, Vector2Int to)> connections;
    public bool spawnTiles;
    public bool hasInterior;
    public GameObject roomPrefab;
}
