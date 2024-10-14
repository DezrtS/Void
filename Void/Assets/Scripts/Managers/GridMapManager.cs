using System.Collections.Generic;
using UnityEngine;

public class GridMapManager : Singleton<GridMapManager>
{
    [SerializeField] private int mapLength = 25;
    [SerializeField] private int mapWidth = 25;
    [SerializeField] private float mapTileSize = 10;
    [SerializeField] private int roomCount = 5;
    [SerializeField] private int seed = 1234;
    [SerializeField] private GameObject mapTilePrefab;
    private readonly List<MapTileCollection> mapTileCollections = new();
    private MapTile[] mapTiles = new MapTile[0];

    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject wall;

    public float MapTileSize { get { return mapTileSize; } }
    public GameObject Floor {  get { return floor; } }
    public GameObject Wall { get { return wall; } }

    private void Start()
    {
        GenerateGridMap();
        GenerateRooms();
    }

    public void SetGridMapParameters(int seed, int mapLength, int mapWidth, float mapTileSize, int roomCount)
    {
        this.seed = seed;
        this.mapLength = mapLength;
        this.mapWidth = mapWidth;
        this.mapTileSize = mapTileSize;
        this.roomCount = roomCount;
    }

    public void ResetGridMap()
    {
        foreach (MapTile tile in mapTiles)
        {
            if (tile)
            {
                Destroy(tile.gameObject);
            }
        }
        mapTileCollections.Clear();
    }

    public void GenerateGridMap()
    {
        ResetGridMap();
        Random.InitState(seed);
        mapTiles = new MapTile[mapLength * mapWidth];
    }

    public void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            GenerateSquareRoom();
        }
    }

    private void GenerateSquareRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2.zero);
        mapTileCollections.Add(mapTileCollection);

        int maxAttempts = 100;
        int attempt = 0;

        while (!success && attempt < maxAttempts)
        {
            attempt++;
            int roomSize = Random.Range(3, 25);

            Vector2 roomPosition = new(Random.Range(0, mapLength - roomSize), Random.Range(0, mapWidth - roomSize));

            Vector2[] mapTilePositions = new Vector2[roomSize * roomSize];
            for (int x = 0; x < roomSize; x++)
            {
                for (int y = 0; y < roomSize; y++)
                {
                    mapTilePositions[x * roomSize + y] = new Vector2(roomPosition.x + x, roomPosition.y + y);
                }
            }

            success = PlaceMapTiles(mapTileCollection, mapTilePositions);
            if (!success)
            {
                Debug.LogWarning($"Failed to generate room, retrying (Attempt: {attempt} / {maxAttempts})");
            }
        }

    }

    public void GenerateHallways()
    {

    }

    public void DecorateMapTiles()
    {

    }

    public void SpawnTiles()
    {
        foreach (MapTile mapTile in mapTiles)
        {
            if (!mapTile)
            {
                continue;
            }
            mapTile.Spawn();
        }
    }

    public bool PlaceMapTile(MapTileCollection mapTileCollection, Vector2 position)
    {
        if (CanPlaceMapTile(position))
        {
            int index = (int) (position.x + position.y * mapLength);
            GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * mapTileSize, 0, position.y * mapTileSize), Quaternion.identity, transform);
            MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
            mapTile.SetMapTile(mapTileCollection, position);
            mapTiles[index] = mapTile;
            mapTileCollection.MapTiles.Add(mapTile);
            return true;
        }
        return false;
    }

    public void ForcePlaceMapTile(MapTileCollection mapTileCollection, Vector2 position)
    {
        int index = (int)(position.x + position.y * mapLength);
        GameObject spawnedMapTile = Instantiate(mapTilePrefab, new Vector3(position.x * mapTileSize, 0, position.y * mapTileSize), Quaternion.identity, transform);
        MapTile mapTile = spawnedMapTile.GetComponent<MapTile>();
        mapTile.SetMapTile(mapTileCollection, position);
        mapTiles[index] = mapTile;
        mapTileCollection.MapTiles.Add(mapTile);
    }

    public bool PlaceMapTiles(MapTileCollection mapTileCollection, Vector2[] positions)
    {
        if (!CanPlaceMapTiles(positions))
        {
            return false;
        }
        foreach (Vector2 position in positions)
        {
            ForcePlaceMapTile(mapTileCollection, position);
        }
        return true;
    }

    public bool CanPlaceMapTile(Vector2 position)
    {
        int index = (int)(position.x + position.y * mapLength);
        if (index >= 0 && index < mapTiles.Length)
        {
            return mapTiles[index] == null;
        }
        return false;
    }

    public bool CanPlaceMapTiles(Vector2[] positions)
    {
        foreach (Vector2 position in positions)
        {
            if (!CanPlaceMapTile(position))
            {
                return false;
            }
        }
        return true;
    }

    public MapTile GetMapTile(Vector2 position)
    {
        int index = (int)(position.x + position.y * mapLength);
        if (index >= 0 && index < mapTiles.Length)
        {
            return mapTiles[index];
        }
        return null;
    }
}
