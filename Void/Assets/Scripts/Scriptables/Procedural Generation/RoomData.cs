using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Procedural Generation/RoomData", order = 1)]
public class RoomData : TileSection
{
    public List<(Vector2Int from, Vector2Int to)> connections;
    public bool SpawnTiles;
    public bool HasInterior;
    public GameObject RoomPrefab;
}
