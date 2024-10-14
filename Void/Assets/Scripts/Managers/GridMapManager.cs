using System.Collections.Generic;
using UnityEngine;

public class GridMapManager : Singleton<GridMapManager>
{
    [Header("Grid Map Parameters")]
    [SerializeField] private int seed = 1234;
    [SerializeField] private int mapLength = 25;
    [SerializeField] private int mapWidth = 25;
    [SerializeField] private float mapTileSize = 10;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 20;
    [SerializeField] private int roomCount = 5;
    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject mapTilePrefab;
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject wall;
    [Space]
    [Header("Misc")]
    [SerializeField] private int maxRoomAttempts = 100;
    [SerializeField] private GameObject debugMarker;

    private readonly List<MapTileCollection> mapTileCollections = new();
    private MapTile[] mapTiles = new MapTile[0];
    private GameObject debugHolder;

    public float MapTileSize { get { return mapTileSize; } }
    public GameObject Floor {  get { return floor; } }
    public GameObject Wall { get { return wall; } }

    public void SetGridMapParameters(int seed, int mapLength, int mapWidth, float mapTileSize, int minRoomSize, int maxRoomSize, int roomCount)
    {
        this.seed = seed;
        this.mapLength = mapLength;
        this.mapWidth = mapWidth;
        this.mapTileSize = mapTileSize;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
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
        Destroy(debugHolder);
        debugHolder = new GameObject("Debug Holder");
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
            GenerateRectangularRoom();
        }
    }

    private void GenerateRectangularRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, new Vector2(1, 1));

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            int roomLength = Random.Range(minRoomSize, maxRoomSize);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);

            Vector2 roomPosition = new(Random.Range(0, mapLength - roomLength), Random.Range(0, mapWidth - roomWidth));
            mapTileCollection.CollectionOrigin = roomPosition;

            Vector2[] mapTilePositions = new Vector2[roomLength * roomWidth];
            for (int y = 0; y < roomWidth; y++)
            {
                for (int x = 0; x < roomLength; x++)
                {
                    mapTilePositions[x + roomLength * y] = new Vector2(roomPosition.x + x, roomPosition.y + y);
                }
            }

            success = PlaceMapTiles(mapTileCollection, mapTilePositions);
            if (!success)
            {
                Debug.LogWarning($"Failed to generate room, retrying (Attempt: {attempt} / {maxRoomAttempts})");
            }
        }

        if (success)
        {
            mapTileCollections.Add(mapTileCollection);
        }
    }

    private void GenerateRandomRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2.zero);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2 roomPosition = new(Random.Range(0, mapLength), Random.Range(0, mapWidth));
            mapTileCollection.CollectionOrigin.Set(roomPosition.x, roomPosition.y);
            Debug.Log(mapTileCollection.CollectionOrigin);
            success = PlaceMapTile(mapTileCollection, roomPosition);
        }

        Debug.Log(mapTileCollection.CollectionOrigin);

        if (!success)
        {
            return;
        }

        attempt = 0;
        success = false;
        int maxTiles = Random.Range(minRoomSize * minRoomSize, maxRoomSize * 2);
        int tilesPlaced = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            MapTile tile = mapTileCollection.MapTiles[Random.Range(0, mapTileCollection.MapTiles.Count)];

            int direction = Random.Range(0, 4);

            if (direction == 0)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position + new Vector2(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 1)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position + new Vector2(0, 1)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 2)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position - new Vector2(1, 0)))
                {
                    tilesPlaced++;
                }
            }
            else if (direction == 3)
            {
                if (PlaceMapTile(mapTileCollection, tile.Position - new Vector2(0, 1)))
                {
                    tilesPlaced++;
                }
            }

            success = tilesPlaced >= maxTiles;
        }

        mapTileCollections.Add(mapTileCollection);
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
        foreach (MapTileCollection mapTileCollection in mapTileCollections)
        {
            Instantiate(debugMarker, new Vector3(mapTileCollection.CollectionOrigin.x * mapTileSize, 0, mapTileCollection.CollectionOrigin.y * mapTileSize), Quaternion.identity, debugHolder.transform);
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