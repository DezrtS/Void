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

    private readonly List<MapTileCollection> mapTileCollections = new();
    private MapTile[] mapTiles = new MapTile[0];

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
            GenerateRandomRoom();
        }
    }

    private void GenerateRectangularRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2.zero);
        mapTileCollections.Add(mapTileCollection);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            int roomLength = Random.Range(minRoomSize, maxRoomSize);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);

            Vector2 roomPosition = new(Random.Range(0, mapLength - roomLength), Random.Range(0, mapWidth - roomWidth));

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

        if (!success)
        {
            mapTileCollections.Remove(mapTileCollection);
        }
    }

    private void GenerateRandomRoom()
    {
        bool success = false;
        MapTileCollection mapTileCollection = new(MapTileCollectionType.None, mapTileCollections.Count, Vector2.zero);
        mapTileCollections.Add(mapTileCollection);

        int attempt = 0;
        while (!success && attempt < maxRoomAttempts)
        {
            attempt++;
            Vector2 roomPosition = new(Random.Range(0, mapLength), Random.Range(0, mapWidth));
            success = PlaceMapTile(mapTileCollection, roomPosition);
        }

        if (!success)
        {
            mapTileCollections.Remove(mapTileCollection);
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
