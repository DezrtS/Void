using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Connection
{
    public Vector2Int from;
    public Vector2Int to;

    public Connection(Vector2Int from, Vector2Int to)
    {
        this.from = from;
        this.to = to;
    }
}

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Procedural Generation/RoomData", order = 1)]
public class RoomData : ScriptableObject, IHoldTilePositions
{
    private Vector2Int size;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private bool spawnTiles;
    [SerializeField] private bool hasInterior;
    [SerializeField] private List<Vector2Int> tilePositions = new List<Vector2Int>();
    [SerializeField] private List<Connection> connections = new List<Connection>();

    public GameObject RoomPrefab => roomPrefab;
    public Vector2Int Size
    {
        get => size;
        set => size = value;
    }
    public bool SpawnTiles
    {
        get => spawnTiles;
        set => spawnTiles = value;
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
