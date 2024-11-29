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
    private Vector2Int gridSize;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private bool spawnFloors;
    [SerializeField] private bool spawnWalls;
    [SerializeField] private bool hasInterior;
    [SerializeField] private List<Vector2Int> tilePositions = new List<Vector2Int>();
    [SerializeField] private List<Connection> connections = new List<Connection>();

    public GameObject RoomPrefab => roomPrefab;
    public Vector2Int GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }
    public bool SpawnFloors
    {
        get => spawnFloors;
        set => spawnFloors = value;
    }
    public bool SpawnWalls
    {
        get => spawnWalls;
        set => spawnWalls = value;
    }
    public bool HasInterior
    {
        get => hasInterior; 
        set => hasInterior = value;
    }
    public List<Vector2Int> TilePositions
    {
        get => tilePositions; 
        set => tilePositions = value;
    }
    public List<Connection> Connections
    {
        get => connections; 
        set => connections = value;
    }
}
